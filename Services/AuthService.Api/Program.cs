// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Api;
using AuthService.Api.Consumers;
using AuthService.Api.Models;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Audit;

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
builder.Services.AddScoped<TokenService>();

builder.Services.AddAuditService();

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
    TokenService tokenService,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IAuditService auditService) =>
{
    var (result, user) = await userService.RegisterUserAsync(userDto);

    if (!result.Succeeded || user is null)
    {
        await auditService.LogErrorAsync(
            "UserRegistration",
            "User",
            null,
            string.Join(", ", result.Errors.Select(e => e.Description)),
            null,
            userDto.Email);

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

    await auditService.LogActionAsync(
        "UserRegistration",
        "User",
        user.Id,
        new { Email = user.Email, Roles = assignedRoles },
        user.Id,
        user.Email);

    return Results.Ok(new
    {
        UserId = user.Id,
        AssignedRoles = assignedRoles,
        Token = token,
        RefreshToken = refreshToken,
        Message = "Registration successful. Please confirm your email.",
    });
})
.WithName("Register")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/login", async (
    LoginModel login,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TokenService tokenService,
    IAuditService auditService) =>
{
    var user = await userManager.FindByEmailAsync(login.Email);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "UserLogin",
            "User",
            null,
            "Invalid credentials - user not found",
            null,
            login.Email);

        return Results.BadRequest("Invalid credentials");
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, false);
    if (!result.Succeeded)
    {
        await auditService.LogErrorAsync(
            "UserLogin",
            "User",
            user.Id,
            "Invalid credentials - wrong password",
            user.Id,
            user.Email);

        return Results.BadRequest("Invalid credentials");
    }

    var roles = await userManager.GetRolesAsync(user);
    var token = await tokenService.GenerateJwtToken(user);
    var refreshToken = await tokenService.GenerateRefreshToken(user);

    await auditService.LogActionAsync(
        "UserLogin",
        "User",
        user.Id,
        new { Email = user.Email },
        user.Id,
        user.Email,
        roles.ToArray());

    return Results.Ok(new AuthResponse
    {
        Token = token,
        RefreshToken = refreshToken,
    });
})
.WithName("Login")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/forgot-password", async (
    ForgotPasswordDto dto,
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "ForgotPassword",
            "User",
            null,
            "User not found",
            null,
            dto.Email);

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

    await auditService.LogActionAsync(
        "ForgotPassword",
        "User",
        user.Id,
        new { Email = user.Email },
        user.Id,
        user.Email);

    return Results.Ok(new { message = "Если email существует, ссылка для сброса пароля была отправлена." });
})
.WithName("ForgotPassword")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/reset-password", async (
    ResetPasswordDto dto,
    UserManager<ApplicationUser> userManager,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "ResetPassword",
            "User",
            null,
            "User not found",
            null,
            dto.Email);

        return Results.BadRequest(new { message = "Неверный запрос." });
    }

    try
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
        if (!result.Succeeded)
        {
            await auditService.LogErrorAsync(
                "ResetPassword",
                "User",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)),
                user.Id,
                user.Email);

            return Results.BadRequest(result.Errors);
        }

        logger.LogInformation("Password reset successful for user {UserId}", user.Id);

        await auditService.LogActionAsync(
            "ResetPassword",
            "User",
            user.Id,
            new { Email = user.Email, Status = "ResetSuccessful" },
            user.Id,
            user.Email);

        return Results.Ok(new { message = "Пароль успешно изменен." });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error resetting password for email {Email}", dto.Email);

        await auditService.LogErrorAsync(
            "ResetPassword",
            "User",
            user.Id,
            ex.Message,
            user.Id,
            user.Email);

        return Results.BadRequest(new { message = "Неверный или просроченный токен." });
    }
})
.WithName("ResetPassword")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/resend-confirmation", async (
    ResendConfirmationDto dto,
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        logger.LogWarning("Resend confirmation requested for non-existent email: {Email}", dto.Email);

        await auditService.LogErrorAsync(
            "ResendConfirmation",
            "User",
            null,
            "User not found",
            null,
            dto.Email);

        return Results.Ok(new { message = "Если email существует, письмо подтверждения отправлено." });
    }

    if (user.EmailConfirmed)
    {
        logger.LogInformation("Email already confirmed for user: {Email}", dto.Email);

        await auditService.LogActionAsync(
            "ResendConfirmation",
            "User",
            user.Id,
            new { Email = user.Email, Status = "AlreadyConfirmed" },
            user.Id,
            user.Email);

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

    await auditService.LogActionAsync(
        "ResendConfirmation",
        "User",
        user.Id,
        new { Email = user.Email },
        user.Id,
        user.Email);

    return Results.Ok(new { message = "Если email существует, письмо подтверждения отправлено." });
})
.WithName("ResendConfirmation")
.AllowAnonymous()
.WithOpenApi();

app.MapPost("/confirm-email", async (
    ConfirmEmailDto dto,
    UserManager<ApplicationUser> userManager,
    IAuditService auditService) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "ConfirmEmail",
            "User",
            null,
            "User not found",
            null,
            dto.Email);

        return Results.BadRequest(new { message = "User not found" });
    }

    if (user.EmailConfirmed)
    {
        await auditService.LogActionAsync(
            "ConfirmEmail",
            "User",
            user.Id,
            new { Email = user.Email, Status = "AlreadyConfirmed" },
            user.Id,
            user.Email);

        return Results.Ok(new { message = "Email already confirmed" });
    }

    try
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            await auditService.LogErrorAsync(
                "ConfirmEmail",
                "User",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)),
                user.Id,
                user.Email);

            return Results.BadRequest(result.Errors);
        }

        await auditService.LogActionAsync(
            "ConfirmEmail",
            "User",
            user.Id,
            new { Email = user.Email, Status = "Confirmed" },
            user.Id,
            user.Email);

        return Results.Ok(new { message = "Email confirmed successfully" });
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "ConfirmEmail",
            "User",
            user.Id,
            ex.Message,
            user.Id,
            user.Email);

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
    UserDTO userDto,
    IAuditService auditService) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "UpdateUser",
            "User",
            userId,
            "User not found");

        return Results.NotFound("User not found");
    }

    var oldRoles = await userManager.GetRolesAsync(user);
    var oldFirstName = user.FirstName;
    var oldLastName = user.LastName;
    var oldMiddleName = user.MiddleName;

    user.LastName = string.IsNullOrEmpty(userDto.LastName) ? userDto.LastName : user.LastName;
    user.FirstName = string.IsNullOrEmpty(userDto.FirstName) ? userDto.FirstName : user.FirstName;
    user.MiddleName = string.IsNullOrEmpty(userDto.MiddleName) ? userDto.MiddleName : user.MiddleName;

    var result = await userService.UpdateUserAsync(userId, userDto);
    if (!result.Succeeded)
    {
        await auditService.LogErrorAsync(
            "UpdateUser",
            "User",
            userId,
            string.Join(", ", result.Errors.Select(e => e.Description)));

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

    await auditService.LogActionAsync(
        "UpdateUser",
        "User",
        userId,
        new
        {
            OldFirstName = oldFirstName,
            NewFirstName = user.FirstName,
            OldLastName = oldLastName,
            NewLastName = user.LastName,
            OldMiddleName = oldMiddleName,
            NewMiddleName = user.MiddleName,
            OldRoles = oldRoles,
            NewRoles = assignedRoles,
            RolesAdded = rolesToAdd,
            RolesRemoved = rolesToRemove,
        },
        user.Id,
        user.Email,
        assignedRoles.ToArray());

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
    UserManager<ApplicationUser> userManager,
    IAuditService auditService) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "DeleteUser",
            "User",
            userId,
            "User not found");

        return Results.NotFound("User not found");
    }

    var currentRoles = await userManager.GetRolesAsync(user);
    var result = await userService.DeleteUserAsync(userId);

    if (!result.Succeeded)
    {
        await auditService.LogErrorAsync(
            "DeleteUser",
            "User",
            userId,
            string.Join(", ", result.Errors.Select(e => e.Description)));

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

    await auditService.LogActionAsync(
        "DeleteUser",
        "User",
        userId,
        new { Email = user.Email, Roles = currentRoles },
        user.Id,
        user.Email);

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

app.MapPost("/refresh", async (
    TokenService tokenService,
    AuthResponse model,
    IAuditService auditService) =>
{
    if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.RefreshToken))
    {
        await auditService.LogErrorAsync(
            "RefreshToken",
            "Token",
            null,
            "Invalid tokens provided");

        return Results.BadRequest("Invalid tokens");
    }

    try
    {
        var response = await tokenService.RefreshTokenAsync(model.Token, model.RefreshToken);

        await auditService.LogActionAsync(
            "RefreshToken",
            "Token",
            null,
            new { TokenRefreshed = true });

        return Results.Ok(response);
    }
    catch (SecurityTokenException ex)
    {
        await auditService.LogErrorAsync(
            "RefreshToken",
            "Token",
            null,
            ex.Message);

        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/add-role", async (
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IPublishEndpoint publishEndpoint,
    string email,
    string role,
    IAuditService auditService) =>
{
    if (!predefinedRoles.Contains(role))
    {
        await auditService.LogErrorAsync(
            "AddRole",
            "Role",
            null,
            $"Invalid role: {role}",
            null,
            email);

        return Results.BadRequest("Invalid role");
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        await auditService.LogErrorAsync(
            "AddRole",
            "User",
            null,
            "User not found",
            null,
            email);

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

    await auditService.LogActionAsync(
        "AddRole",
        "User",
        user.Id,
        new { Email = user.Email, Role = role },
        user.Id,
        user.Email);

    return Results.Ok($"Role '{role}' added to {email}");
})
.RequireAuthorization();

app.MapGet("/userId", async (
    UserManager<ApplicationUser> userManager,
    string userName) =>
{
    var user = await userManager.FindByNameAsync(userName);
    return user?.Id;
});

app.MapGet("/users", async (
    UserManager<ApplicationUser> userManager,
    IAuditService auditService) =>
{
    try
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

        await auditService.LogActionAsync(
            "GetUsers",
            "User",
            null,
            new { Count = userDtos.Count });

        return Results.Ok(userDtos);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "GetUsers",
            "User",
            null,
            ex.Message);

        throw;
    }
})
.RequireAuthorization("AdminOnly");

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