// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Api;
using AuthService.Api.Consumers;
using AuthService.Api.Models;
using Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var predefinedRoles = RoleNames.GetAllRoleNames();

var builder = WebApplication.CreateBuilder(args);

var currentEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Default";

builder.Services.AddDbContext<AuthDbContext>(
    options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString(currentEnvironment)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

byte[] key = Encoding.UTF8.GetBytes(
    builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing."));

builder.Services.AddAuthentication(
        cfg =>
        {
            cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
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

builder.Services.AddAuthorization(
    options => { options.AddPolicy("AdminOnly", policy => policy.RequireRole("Администратор")); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
            });

        c.AddSecurityRequirement(
            new OpenApiSecurityRequirement
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

builder.Services.AddMassTransit(
    x =>
    {
        x.AddConsumer<UserWithRoleActionConsumer>();

        x.UsingRabbitMq(
            (context, cfg) =>
            {
                cfg.Host(
                    builder.Configuration["RabbitMQ:Host"],
                    "/",
                    h =>
                    {
                        h.Username(builder.Configuration["RabbitMQ:Username"]);
                        h.Password(builder.Configuration["RabbitMQ:Password"]);
                    });

                cfg.Message<UserCreatedEvent>(x => x.SetEntityName("user-events"));

                cfg.ReceiveEndpoint("user-with-role-events", e =>
                {
                    e.ConfigureConsumer<UserWithRoleActionConsumer>(context);
                });
            });
    });

builder.Services.AddScoped<TokenService>();

var app = builder.Build();

app.UseCors("CorsPolicy");

// Enable Swagger in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost(
    "/register",
    async (
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IPublishEndpoint publishEndpoint,
        ApplicationUserDTO userDto,
        TokenService tokenService) =>
    {
        var user = new ApplicationUser
        {
            UserName = userDto.Email, Email = userDto.Email, FirstName = userDto.FirstName, LastName = userDto.LastName,
            MiddleName = userDto.MiddleName,
        };
        var result = await userManager.CreateAsync(user, userDto.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors);
        }

        var assignedRoles = new List<string>();
        if (userDto.Roles?.Length > 0)
        {
            foreach (var role in userDto.Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }

                await userManager.AddToRoleAsync(user, role);
                assignedRoles.Add(role);
            }
        }

        await publishEndpoint.Publish(
            new UserCreatedEvent(
                user.Id,
                user.UserName,
                userDto.FirstName,
                userDto.LastName,
                userDto.MiddleName,
                assignedRoles.ToArray(),
                DateTime.UtcNow));

        var token = await tokenService.GenerateJwtToken(user);
        var refreshToken = await tokenService.GenerateRefreshToken(user);

        return Results.Ok(
            new
            {
                UserId = user.Id,
                AssignedRoles = assignedRoles,
                Token = token,
                RefreshToken = refreshToken,
            });
    });

app.MapPost("/login", async (LoginModel login, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TokenService tokenService) =>
{
    var user = await userManager.FindByEmailAsync(login.Email);
    if (user == null)
    {
        return Results.BadRequest("Invalid credentials");
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, false);
    if (!result.Succeeded)
    {
        return Results.BadRequest("Invalid credentials");
    }

    var token = await tokenService.GenerateJwtToken(user);
    var refreshToken = await tokenService.GenerateRefreshToken(user);

    return Results.Ok(new AuthResponse
    {
        Token = token,
        RefreshToken = refreshToken,
    });
});

app.MapPost("/refresh", async (HttpContext context, TokenService tokenService, AuthResponse model) =>
{
    var token = model.Token;
    var refreshToken = model.RefreshToken;

    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
    {
        return Results.BadRequest("Invalid tokens");
    }

    try
    {
        var response = await tokenService.RefreshTokenAsync(token, refreshToken);
        return Results.Ok(response);
    }
    catch (SecurityTokenException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// **Add Role to User (Admin Only)**
app.MapPost(
    "/add-role",
    async (
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        string email,
        string role) =>
    {
        if (!predefinedRoles.Contains(role))
        {
            return Results.BadRequest("Invalid role");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Results.NotFound("User not found");
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        await userManager.AddToRoleAsync(user, role);
        return Results.Ok($"Role '{role}' added to {email}");
    }).RequireAuthorization();

app.MapGet("/user", async (UserManager<ApplicationUser> userManager, string userId) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return null;
    }

    var roles = await userManager.GetRolesAsync(user);
    var model = new UserDTO()
    {
        UserId = user.Id,
        Email = user.Email ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        MiddleName = user.MiddleName,
        Roles = roles.ToArray(),
    };
    return model;
});

app.MapGet("/userId", async (UserManager<ApplicationUser> userManager, string userName) =>
{
    var user = await userManager.FindByNameAsync(userName);
    if (user == null)
    {
        return null;
    }

    return user.Id;
});

app.MapGet("/users", async (UserManager<ApplicationUser> userManager) =>
{
    var users = await userManager.Users.ToListAsync();

    var userDtos = new List<UserDTO>();

    foreach (var user in users)
    {
        var roles = await userManager.GetRolesAsync(user);
        userDtos.Add(new UserDTO
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            Roles = roles.ToArray(),
        });
    }

    return Results.Ok(userDtos);
}).RequireAuthorization("AdminOnly");

// **Ensure Roles Exist in Database**
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in predefinedRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();