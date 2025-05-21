// <copyright file="AuthServiceTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Tests;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Api;
using AuthService.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

/// <summary>
/// Unit tests for <see cref="AuthDbContext"/> using in-memory database.
/// </summary>
[TestFixture]
public class AuthServiceTests : IAsyncDisposable
{
    private AuthDbContext dbContext;
    private UserManager<ApplicationUser> userManager;
    private RoleManager<IdentityRole> roleManager;

    /// <summary>
    /// Initializes the test environment before any tests run.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new AuthDbContext(options);
        await this.dbContext.Database.EnsureCreatedAsync();

        var store = new UserStore<ApplicationUser, IdentityRole, AuthDbContext>(this.dbContext);

        var identityOptions = new IdentityOptions
        {
            Password =
            {
                RequiredLength = 6,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
            },
        };

        var serviceProvider = new Mock<IServiceProvider>().Object;
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>().Object;

        this.userManager = new UserManager<ApplicationUser>(
            store,
            Options.Create(identityOptions),
            new PasswordHasher<ApplicationUser>(),
            new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() },
            new List<IPasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() },
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            serviceProvider,
            logger);

        var roleStore = new RoleStore<IdentityRole>(this.dbContext);
        this.roleManager = new RoleManager<IdentityRole>(
            roleStore,
            new List<IRoleValidator<IdentityRole>> { new RoleValidator<IdentityRole>() },
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null);
    }

    /// <summary>
    /// Tests that a user can be successfully created in the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanCreateUser()
    {
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Johnov",
            UserName = "test@example.com",
            Email = "test@example.com",
            EmailConfirmed = true,
        };

        var result = await this.userManager.CreateAsync(user, "SecurePassword123!");
        Assert.That(result.Succeeded, Is.True);

        var dbUser = await this.userManager.FindByEmailAsync("test@example.com");
        Assert.That(dbUser, Is.Not.Null);
    }

    /// <summary>
    /// Tests that a role can be successfully assigned to a user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanAddUserToRole()
    {
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Johnov",
            UserName = "roleuser@example.com",
            Email = "roleuser@example.com",
        };

        await this.userManager.CreateAsync(user, "SecurePassword123!");

        const string roleName = "TestRole";
        await this.roleManager.CreateAsync(new IdentityRole(roleName));

        var result = await this.userManager.AddToRoleAsync(user, roleName);
        Assert.That(result.Succeeded, Is.True);

        var roles = await this.userManager.GetRolesAsync(user);
        Assert.That(roles, Contains.Item(roleName));
    }

    /// <summary>
    /// Tests that a valid JWT token can be generated and contains expected claims.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanGenerateAndValidateJwtToken()
    {
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Johnov",
            UserName = "tokenuser@example.com",
            Email = "tokenuser@example.com",
        };

        await this.userManager.CreateAsync(user, "SecurePassword123!");

        var tokenService = this.CreateTokenService();
        var token = await tokenService.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Multiple(
            () =>
        {
            Assert.That(jwtToken, Is.Not.Null);
            Assert.That(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Email && c.Value == user.Email), Is.True);
        });
    }

    /// <summary>
    /// Tests that multiple roles can be assigned to a single user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanCreateUserWithMultipleRoles()
    {
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Johnov",
            UserName = "multirole@example.com",
            Email = "multirole@example.com",
        };

        await this.userManager.CreateAsync(user, "SecurePassword123!");

        var roles = new[] { "Role1", "Role2", "Role3" };
        foreach (var role in roles)
        {
            await this.roleManager.CreateAsync(new IdentityRole(role));
        }

        foreach (var role in roles)
        {
            await this.userManager.AddToRoleAsync(user, role);
        }

        var userRoles = await this.userManager.GetRolesAsync(user);
        Assert.That(userRoles, Is.EquivalentTo(roles));
    }

    /// <summary>
    /// Tests that user registration fails with invalid password.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegistrationFailsWithInvalidPassword()
    {
        var userDto = new ApplicationUserDTO
        {
            Email = "invalidpass@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "short",
        };

        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
        };

        var result = await this.userManager.CreateAsync(user, userDto.Password);

        Assert.Multiple(
            () =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.Any(e => e.Code == "PasswordTooShort"), Is.True);
        });
    }

    /// <summary>
    /// Tests that token refresh works with valid tokens.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanRefreshTokenWithValidTokens()
    {
        var user = new ApplicationUser
        {
            UserName = "refreshuser@example.com",
            Email = "refreshuser@example.com",
            FirstName = "Refreshuser",
            LastName = "Name",
        };

        await this.userManager.CreateAsync(user, "Password123!");

        var tokenService = this.CreateTokenService();
        var token = await tokenService.GenerateJwtToken(user);
        var refreshToken = await tokenService.GenerateRefreshToken(user);

        var refreshedResponse = await tokenService.RefreshTokenAsync(token, refreshToken);

        Assert.Multiple(
            () =>
        {
            Assert.That(refreshedResponse, Is.Not.Null);
            Assert.That(refreshedResponse.Token, Is.Not.Null);
            Assert.That(refreshedResponse.RefreshToken, Is.Not.Null);
        });
    }

    /// <summary>
    /// Tests that token refresh fails with invalid refresh token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RefreshTokenFailsWithInvalidRefreshToken()
    {
        var user = new ApplicationUser
        {
            UserName = "invalidrefresh@example.com",
            Email = "invalidrefresh@example.com",
            FirstName = "Invalidrefresh",
            LastName = "Name",
        };

        await this.userManager.CreateAsync(user, "Password123!");

        var tokenService = this.CreateTokenService();
        var token = await tokenService.GenerateJwtToken(user);

        Assert.ThrowsAsync<SecurityTokenException>(
            async () => await tokenService.RefreshTokenAsync(token, "invalid-refresh-token"));
    }

    /// <summary>
    /// Tests that user cannot be assigned invalid role.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CannotAssignInvalidRoleToUser()
    {
        var user = new ApplicationUser
        {
            UserName = "invalidrole@example.com",
            Email = "invalidrole@example.com",
            FirstName = "Invalidrole",
            LastName = "Name",
        };

        await this.userManager.CreateAsync(user, "Password123!");

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await this.userManager.AddToRoleAsync(user, "NonExistentRole"));

        Assert.That(exception.Message, Does.Contain("does not exist").IgnoreCase);
    }

    /// <summary>
    /// Tests that user can be successfully deleted.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanDeleteUser()
    {
        var user = new ApplicationUser
        {
            UserName = "todelete@example.com",
            Email = "todelete@example.com",
            FirstName = "Todelete",
            LastName = "Name",
        };

        await this.userManager.CreateAsync(user, "Password123!");
        var userId = user.Id;

        var deleteResult = await this.userManager.DeleteAsync(user);

        await Assert.MultipleAsync(
            async () =>
            {
                Assert.That(deleteResult.Succeeded, Is.True);
                Assert.That(await this.userManager.FindByIdAsync(userId), Is.Null);
            });
    }

    /// <summary>
    /// Tests that user information can be successfully updated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CanUpdateUserInformation()
    {
        var user = new ApplicationUser
        {
            UserName = "original@example.com",
            Email = "original@example.com",
            FirstName = "Original",
            LastName = "Name",
        };

        await this.userManager.CreateAsync(user, "Password123!");

        user.FirstName = "Updated";
        user.LastName = "Name";
        user.Email = "updated@example.com";
        user.UserName = "updated@example.com";

        var updateResult = await this.userManager.UpdateAsync(user);

        await Assert.MultipleAsync(
            async () =>
        {
            Assert.That(updateResult.Succeeded, Is.True);
            var updatedUser = await this.userManager.FindByIdAsync(user.Id);
            Assert.That(updatedUser?.FirstName, Is.EqualTo("Updated"));
            Assert.That(updatedUser?.Email, Is.EqualTo("updated@example.com"));
        });
    }

    /// <summary>
    /// Performs cleanup of test resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.dbContext.DisposeAsync();
    }

    /// <summary>
    /// Creates an instance of <see cref="TokenService"/> for testing.
    /// </summary>
    /// <returns>The configured <see cref="TokenService"/>.</returns>
    private TokenService CreateTokenService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Key"] = "YourTestKeyMustBeAtLeast128BitsLong",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpireMinutes"] = "30",
            })
            .Build();

        return new TokenService(configuration, this.dbContext, this.userManager);
    }
}