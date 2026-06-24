// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NotificationService.Api.Consumers;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;
using NotificationService.Api.Models.DTOs;
using NotificationService.Api.Services;

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

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthentication("GatewayAuth")
    .AddScheme<AuthenticationSchemeOptions, GatewayAuthHandler.GatewayAuthHandler>("GatewayAuth", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Администратор"));
});

// Current environment
var currentEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Default";

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
    x.AddConsumer<PasswordResetRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var username = builder.Configuration["RabbitMQ:Username"] ?? "admin";
        var password = builder.Configuration["RabbitMQ:Password"] ?? "admin123";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
            h.Heartbeat(TimeSpan.FromSeconds(30));
            h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
        });

        cfg.ReceiveEndpoint("password-reset-events", e =>
        {
            e.UseMessageRetry(retry =>
            {
                retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));
                retry.Ignore<ValidationException>();
                retry.Ignore<ArgumentException>();
            });

            e.PrefetchCount = 5;
            e.ConcurrentMessageLimit = 3;

            e.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
        });
    });
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

 // Send email endpoint
app.MapPost("/api/email/send", async (SendEmailDto dto, IEmailService emailService) =>
{
    if (dto == null)
    {
        return Results.BadRequest(new { message = "Request body is required" });
    }

    if (string.IsNullOrEmpty(dto.To) ||
        string.IsNullOrEmpty(dto.Subject) ||
        string.IsNullOrEmpty(dto.Body))
    {
        return Results.BadRequest(new { message = "To, Subject and Body are required" });
    }

    var result = await emailService.SendEmailAsync(dto);

    return result
        ? Results.Ok(new { message = "Email sent successfully" })
        : Results.BadRequest(new { message = "Failed to send email" });
})
.WithName("SendEmail")
.WithOpenApi();

// Send password reset email endpoint
app.MapPost("/api/email/password-reset", async (PasswordResetRequestDto request, IEmailService emailService) =>
{
    if (request == null)
    {
        return Results.BadRequest(new { message = "Request body is required" });
    }

    if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.ResetLink))
    {
        return Results.BadRequest(new { message = "Email and ResetLink are required" });
    }

    var result = await emailService.SendPasswordResetEmailAsync(request);

    return result
        ? Results.Ok(new { message = "Password reset email sent successfully" })
        : Results.BadRequest(new { message = "Failed to send password reset email" });
})
.WithName("SendPasswordResetEmail")
.AllowAnonymous()
.WithOpenApi();

// Test email endpoint
app.MapPost("/api/email/test", async (IEmailService emailService, string email) =>
{
    var testDto = new SendEmailDto
    {
        To = email,
        Subject = "Тестовое письмо",
        Body = "<h1>Это тестовое письмо</h1><p>Если вы получили это письмо, SMTP настроен правильно.</p>",
    };

    var result = await emailService.SendEmailAsync(testDto);

    return result
        ? Results.Ok(new { message = "Test email sent" })
        : Results.BadRequest(new { message = "Failed to send test email" });
})
.WithName("SendTestEmail")
.AllowAnonymous()
.WithOpenApi();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();