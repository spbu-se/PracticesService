// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
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
using Microsoft.AspNetCore.WebUtilities;
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
var gatewayBasePath = builder.Configuration["Swagger:GatewayBasePath"] ?? "/api";

builder.Services.AddSwaggerGen(c =>
{
        c.AddServer(new OpenApiServer
        {
            Url = gatewayBasePath,
            Description = "Gateway endpoint",
        });

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
                        h.Heartbeat(TimeSpan.FromSeconds(30));
                        h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                    });

                cfg.Message<UserCreatedEvent>(x => x.SetEntityName("user-events"));
                cfg.Message<UserEditedEvent>(x => x.SetEntityName("user-edited-events"));
                cfg.Message<PasswordResetRequestedEvent>(x => x.SetEntityName("password-reset-events"));
                cfg.Message<EmailConfirmationRequestedEvent>(x => x.SetEntityName("email-confirmation-events"));

                cfg.ReceiveEndpoint("user-with-role-events", e =>
                {
                    e.UseMessageRetry(retry =>
                    {
                        retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));
                        retry.Ignore<ValidationException>();
                        retry.Ignore<ArgumentException>();
                    });

                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;

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

app.MapPost("/forgot-password", async (
    ForgotPasswordDto dto,
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<Program> logger) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        return Results.Ok(new { message = "Если email существует, ссылка для сброса пароля была отправлена." });
    }

    var token = await userManager.GeneratePasswordResetTokenAsync(user);
    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:8000/practices-service";
    var resetLink = $"{frontendUrl}/reset-password?token={encodedToken}&email={dto.Email}";
    await publishEndpoint.Publish(new PasswordResetRequestedEvent(
        UserId: user.Id,
        Email: user.Email,
        ResetLink: resetLink,
        UserName: user.UserName,
        RequestedAt: DateTime.UtcNow));

    logger.LogInformation("Password reset requested for user {UserId}", user.Id);

    return Results.Ok(new { message = "Если email существует, ссылка для сброса пароля была отправлена." });
})
.WithName("ForgotPassword")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/reset-password", async (
    ResetPasswordDto dto,
    UserManager<ApplicationUser> userManager,
    ILogger<Program> logger) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        return Results.BadRequest(new { message = "Неверный запрос." });
    }

    try
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors);
         }

        logger.LogInformation("Password reset successful for user {UserId}", user.Id);

        return Results.Ok(new { message = "Пароль успешно изменен." });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error resetting password for email {Email}", dto.Email);
        return Results.BadRequest(new { message = "Неверный или просроченный токен." });
    }
})
.WithName("ResetPassword")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/register", async (
    UserService userService,
    IPublishEndpoint publishEndpoint,
    ApplicationUserDTO userDto,
    TokenService tokenService,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) =>
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
        user.Email,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        assignedRoles.ToArray(),
        DateTime.UtcNow));

    var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
    var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:8000/practices-service";
    var confirmLink = $"{frontendUrl}/confirm-email?token={encodedToken}&email={user.Email}";

    await publishEndpoint.Publish(new EmailConfirmationRequestedEvent(
        UserId: user.Id,
        Email: user.Email!,
        UserName: user.UserName!,
        ConfirmLink: confirmLink,
        RequestedAt: DateTime.UtcNow));

    var token = await tokenService.GenerateJwtToken(user);
    var refreshToken = await tokenService.GenerateRefreshToken(user);

    return Results.Ok(new
    {
        UserId = user.Id,
        AssignedRoles = assignedRoles,
        Token = token,
        RefreshToken = refreshToken,
        Message = "Registration successful. Please confirm your email.",
    });
});

app.MapPost("/resend-confirmation", async (
        ResendConfirmationDto dto,
        UserManager<ApplicationUser> userManager,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<Program> logger) =>
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            logger.LogWarning("Resend confirmation requested for non-existent email: {Email}", dto.Email);
            return Results.Ok(new { message = "Если email существует, письмо подтверждения отправлено." });
        }

        if (user.EmailConfirmed)
        {
            logger.LogInformation("Email already confirmed for user: {Email}", dto.Email);
            return Results.BadRequest(new { message = "Email уже подтвержден. Вы можете войти в систему." });
        }

        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
        var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:8000";
        var confirmLink = $"{frontendUrl}/confirm-email?token={encodedToken}&email={user.Email}";

        await publishEndpoint.Publish(new EmailConfirmationRequestedEvent(
            UserId: user.Id,
            Email: user.Email!,
            UserName: user.UserName!,
            ConfirmLink: confirmLink,
            RequestedAt: DateTime.UtcNow));

        logger.LogInformation("Resent confirmation email for user {UserId}", user.Id);

        return Results.Ok(new { message = "Если email существует, письмо подтверждения отправлено." });
    })
    .WithName("ResendConfirmation")
    .AllowAnonymous()
    .WithOpenApi();

app.MapPost("/confirm-email", async (
        ConfirmEmailDto dto,
        UserManager<ApplicationUser> userManager) =>
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Results.BadRequest(new { message = "User not found" });
        }

        if (user.EmailConfirmed)
        {
            return Results.Ok(new { message = "Email already confirmed" });
        }

        try
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            return Results.Ok(new { message = "Email confirmed successfully" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = "Invalid or expired token" });
        }
    })
    .WithName("ConfirmEmail")
    .AllowAnonymous()
    .WithOpenApi();

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

    var rolesToAdd = userDto.Roles.Except(currentRoles).ToList();
    var rolesToRemove = currentRoles.Except(userDto.Roles).ToList();
    if (rolesToRemove.Any())
    {
        await userManager.RemoveFromRolesAsync(user, rolesToRemove);
    }

    if (rolesToAdd.Any())
    {
        await userManager.AddToRolesAsync(user, rolesToAdd);
    }

    var assignedRoles = await userService.AssignRolesAsync(user, userDto.Roles);

    await publishEndpoint.Publish(new UserEditedEvent(
        user.Id,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        rolesToAdd,
        rolesToRemove,
        currentRoles,
        DateTime.UtcNow));
    return Results.Ok(new
    {
        UserId = user.Id,
        AssignedRoles = assignedRoles,
    });
});

app.MapDelete("/users/{userId}", async (
    string userId,
    IPublishEndpoint publishEndpoint,
    UserService userService,
    UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    var currentRoles = await userManager.GetRolesAsync(user);
    var result = await userService.DeleteUserAsync(userId);

    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }

    await publishEndpoint.Publish(new UserEditedEvent(
        user.Id,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        null,
        currentRoles,
        null,
        DateTime.UtcNow));

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
        user.Email!,
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