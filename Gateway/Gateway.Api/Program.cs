// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway API", Version = "v1" });

    c.SwaggerDoc("core", new OpenApiInfo { Title = "Core Service", Version = "v1" });
    c.SwaggerDoc("auth", new OpenApiInfo { Title = "Auth Service", Version = "v1" });
    c.SwaggerDoc("practice-entities", new OpenApiInfo { Title = "Practice Entities", Version = "v1" });
    c.SwaggerDoc("notification", new OpenApiInfo { Title = "Notification Service", Version = "v1" });
    c.SwaggerDoc("audit", new OpenApiInfo { Title = "Audit Service", Version = "v1" });
});

byte[] key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing."));

builder.Services.AddAuthentication(cfg =>
{
    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Bearer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");

        c.SwaggerEndpoint("/core-swagger/swagger/v1/swagger.json", "Core Service");
        c.SwaggerEndpoint("/auth-swagger/swagger/v1/swagger.json", "Auth Service");
        c.SwaggerEndpoint("/practice-entities-swagger/swagger/v1/swagger.json", "Practice Entities Service");
        c.SwaggerEndpoint("/notification-swagger/swagger/v1/swagger.json", "Notification Service");
        c.SwaggerEndpoint("/audit-swagger/swagger/v1/swagger.json", "Audit Service");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
