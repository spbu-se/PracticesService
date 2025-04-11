// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using CoreService;
using CoreService.Core;
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
builder.Services.AddSwaggerGen(c =>
{
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

app.MapGet("api/me", (HttpContext context) =>
{
    var username = context.User.Identity?.Name;
    return username;
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
