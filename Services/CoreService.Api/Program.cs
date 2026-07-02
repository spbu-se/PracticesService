// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Contracts;
using CoreService.Api.Consumers;
using CoreService.Api.Core;
using CoreService.Api.Endpoints;
using CoreService.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var gatewayBasePath = builder.Configuration["Swagger:GatewayBasePath"] ?? "/api";

builder.Services.AddSwaggerGen(c =>
{
    c.AddServer(new OpenApiServer
    {
        Url = gatewayBasePath,
        Description = "Gateway endpoint",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            new List<string>()
        },
    });
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri("http://auth.api:8080/");
});

builder.Services.AddAuthentication("GatewayAuth")
    .AddScheme<AuthenticationSchemeOptions, GatewayAuthHandler.GatewayAuthHandler>("GatewayAuth", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Администратор"));
});

// Current environment
var currentEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Default";

// Register db context
builder.Services.AddDbContext<CoreContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString(currentEnvironment)));

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "CorsPolicy",
            policyBuilder => policyBuilder
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed((_) => true)
                .AllowAnyHeader());
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<UserEditedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
            h.Heartbeat(TimeSpan.FromSeconds(30));
            h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
        });

        cfg.ReceiveEndpoint("user-events", e =>
        {
            e.UseMessageRetry(retry =>
            {
                retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));
                retry.Ignore<ValidationException>();
                retry.Ignore<ArgumentException>();
            });

            e.PrefetchCount = 10;
            e.ConcurrentMessageLimit = 5;

            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-edited-events", e =>
        {
            e.UseMessageRetry(retry =>
            {
                retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));
                retry.Ignore<ValidationException>();
                retry.Ignore<ArgumentException>();
            });

            e.PrefetchCount = 10;
            e.ConcurrentMessageLimit = 5;

            e.ConfigureConsumer<UserEditedConsumer>(context);
        });

        cfg.Message<UserWithRoleActionEvent>(x => x.SetEntityName("user-with-role-events"));
        cfg.Message<ThemeArchivedEvent>(x => x.SetEntityName("theme-events"));
    });
});

builder.Services.AddScoped<UserResolverService>();

var app = builder.Build();

app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Themes Endpoints
app.MapGroup("api/themes/").ThemesGroup().WithTags("Themes");

// Consultants Endpoints
app.MapGroup("api/consultants/").ConsultantsGroup().WithTags("Consultants");

// Groups Endpoints
app.MapGroup("api/groups/").GroupsGroup().WithTags("Groups");

// Lecturers Endpoints
app.MapGroup("api/lecturers/").LecturersGroup().WithTags("Lecturers");

// Practices Endpoints
app.MapGroup("api/practices/").PracticesGroup().WithTags("Practices");

// Students Endpoints
app.MapGroup("api/students/").StudentsGroup().WithTags("Students");

app.MapGet("api/me", async (HttpContext context, UserResolverService userResolver) =>
{
    var user = context.User;

    var username = user.Identity?.Name ?? string.Empty;
    var firstName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
    var lastName = user.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
    var middleName = user.FindFirst("middle_name")?.Value; // Custom claim
    var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

    var userId = await userResolver.GetUserIdAsync(username);
    return new UserDTO()
    {
        UserId = userId ?? string.Empty,
        UserName = username,
        FirstName = firstName,
        LastName = lastName,
        MiddleName = middleName,
        Email = email,
        Roles = roles,
    };
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
