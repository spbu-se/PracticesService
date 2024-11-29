// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using CoreService;
using CoreService.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Current environment
var currentEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Default";

// Register db context
builder.Services.AddDbContext<CoreContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString(currentEnvironment)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mock user database
var mockUsers = new List<object>
{
    new { Id = 1, Name = "John Doe", Email = "student@example.com", Roles = new[] { "Student" } },
    new { Id = 2, Name = "Jane Smith", Email = "lecturer@example.com", Roles = new[] { "Lecturer" } },
    new { Id = 3, Name = "Admin User", Email = "admin@example.com", Roles = new[] { "Admin", "Lecturer" } },
};

// Endpoint to get all users
app.MapGet("/api/authmock/users", () =>
{
    return Results.Ok(mockUsers);
});

// Endpoint to validate token and return user info
app.MapGet("/api/authmock/validate", (string token) =>
{
    // Simulate token-to-user mapping (mocked)
    var user = token switch
    {
        "token-student" => mockUsers[0],
        "token-lecturer" => mockUsers[1],
        "token-admin" => mockUsers[2],
        _ => null,
    };

    if (user == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(user);
});

// Themes Endpoints
app.MapGroup("api/core/themes/").ThemesGroup().WithTags("Themes");

app.Run();
