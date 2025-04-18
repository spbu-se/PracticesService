// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

var predefinedRoles = new[]
    {
        "Студент", "Научный руководитель", "Консультант", "Руководитель практики", "Рецензент", "Администратор",
    };

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

var app = builder.Build();

app.UseCors("CorsPolicy");

// Enable Swagger in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// **User Registration**
app.MapPost(
    "/register",
    async (
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IPublishEndpoint publishEndpoint,
        string email,
        string password,
        ApplicationUserDTO userDto) =>
    {
        var user = new ApplicationUser
        {
            UserName = email, Email = email, FirstName = userDto.FirstName, LastName = userDto.LastName,
            MiddleName = userDto.MiddleName,
        };
        var result = await userManager.CreateAsync(user, password);

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

        return Results.Ok(
            new
            {
                UserId = user.Id,
                AssignedRoles = assignedRoles,
            });
    });

// **Login & Token Generation**
app.MapPost(
    "/login",
    async (UserManager<ApplicationUser> userManager, LoginModel model) =>
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
        {
            return Results.Unauthorized();
        }

        var userRoles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName ?? "UnknownUser"),
            new Claim(ClaimTypes.Email, user.Email ?? "unknown@example.com"),
        };

        // Add roles to token
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: builder.Configuration["Jwt:Issuer"],
            audience: builder.Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

        return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
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