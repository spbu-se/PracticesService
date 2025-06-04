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
using Microsoft.AspNetCore.Mvc;
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

builder.Services.AddScoped<UserService>();

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

if (Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true")
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}

app.UseCors("CorsPolicy");

// Enable Swagger in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/register", async (
    UserService userService,
    IPublishEndpoint publishEndpoint,
    ApplicationUserDTO userDto,
    TokenService tokenService) =>
{
    var (result, user) = await userService.RegisterUserAsync(userDto);

    if (!result.Succeeded || user is null)
    {
        return Results.BadRequest(result.Errors);
    }

    var assignedRoles = await userService.AssignRolesAsync(user, userDto.Roles);

    await publishEndpoint.Publish(new UserCreatedEvent(
        user.Id,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        assignedRoles.ToArray(),
        DateTime.UtcNow));

    var token = await tokenService.GenerateJwtToken(user);
    var refreshToken = await tokenService.GenerateRefreshToken(user);

    return Results.Ok(new
    {
        UserId = user.Id,
        AssignedRoles = assignedRoles,
        Token = token,
        RefreshToken = refreshToken,
    });
});

app.MapPut("/users/{userId}", async (
    string userId,
    UserService userService,
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
    UserDTO userDto) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    user.LastName = string.IsNullOrEmpty(userDto.LastName) ? userDto.LastName : user.LastName;
    user.FirstName = string.IsNullOrEmpty(userDto.FirstName) ? userDto.FirstName : user.FirstName;
    user.MiddleName = string.IsNullOrEmpty(userDto.MiddleName) ? userDto.MiddleName : user.MiddleName;

    var result = await userService.UpdateUserAsync(userId, userDto);
    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }

    var currentRoles = await userManager.GetRolesAsync(user);
    await userManager.RemoveFromRolesAsync(user, currentRoles);

    var assignedRoles = await userService.AssignRolesAsync(user, userDto.Roles);

    await publishEndpoint.Publish(new UserCreatedEvent(
        user.Id,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        assignedRoles.ToArray(),
        DateTime.UtcNow));

    return Results.Ok(new
    {
        UserId = user.Id,
        AssignedRoles = assignedRoles,
    });
});

app.MapDelete("/users/{userId}", async (
    string userId,
    UserService userService) =>
{
    var result = await userService.DeleteUserAsync(userId);

    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }

    return Results.Ok(new { UserId = userId });
});

app.MapGet("/users/{userId}", async (
    string userId,
    UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    var roles = await userManager.GetRolesAsync(user);

    return Results.Ok(new
    {
        UserId = user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        Roles = roles,
    });
});

app.MapPost("/login", async (
    LoginModel login,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TokenService tokenService) =>
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

app.MapPost("/refresh", async (
    TokenService tokenService,
    AuthResponse model) =>
{
    if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.RefreshToken))
    {
        return Results.BadRequest("Invalid tokens");
    }

    try
    {
        var response = await tokenService.RefreshTokenAsync(model.Token, model.RefreshToken);
        return Results.Ok(response);
    }
    catch (SecurityTokenException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/add-role", async (
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IPublishEndpoint publishEndpoint,
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

    await publishEndpoint.Publish(new UserCreatedEvent(
        user.Id,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        new[] { role },
        DateTime.UtcNow));
    return Results.Ok($"Role '{role}' added to {email}");
}).RequireAuthorization();

app.MapGet(
    "/userId",
    async (
    UserManager<ApplicationUser> userManager,
    string userName) =>
{
    var user = await userManager.FindByNameAsync(userName);
    return user?.Id;
});

app.MapGet(
    "/users",
    async (
    UserManager<ApplicationUser> userManager) =>
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

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in predefinedRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = builder.Configuration["ADMIN_EMAIL"];
    var adminPassword = builder.Configuration["ADMIN_PASSWORD"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Администратор");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Администратор"))
            {
                await userManager.AddToRoleAsync(adminUser, "Администратор");
            }
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();