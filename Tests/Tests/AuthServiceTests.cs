// <copyright file="AuthServiceTests.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

/// <summary>
/// Integration tests for <see cref="AuthDbContext"/>.
/// </summary>
[TestFixture]
public class AuthServiceTests : IAsyncDisposable
{
    private PostgreSqlContainer postgresContainer;
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
        this.postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await this.postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(this.postgresContainer.GetConnectionString())
            .Options;

        this.dbContext = new AuthDbContext(options);
        await this.dbContext.Database.EnsureCreatedAsync();

        var store = new UserStore<ApplicationUser, IdentityRole, AuthDbContext>(this.dbContext);

        this.userManager = new UserManager<ApplicationUser>(
            store,
            null,
            new PasswordHasher<ApplicationUser>(),
            new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() },
            new List<IPasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() },
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            null);

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

        const string RoleName = "TestRole";
        await this.roleManager.CreateAsync(new IdentityRole(RoleName));

        var result = await this.userManager.AddToRoleAsync(user, RoleName);
        Assert.That(result.Succeeded, Is.True);
        var roles = await this.userManager.GetRolesAsync(user);
        Assert.That(roles, Contains.Item(RoleName));
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

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Key"] = "YourTestKeyMustBeAtLeast128BitsLong",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpireMinutes"] = "30",
            })
            .Build();

        var tokenService = new TokenService(configuration, this.dbContext);
        var token = tokenService.GenerateJwtToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.That(jwtToken, Is.Not.Null);
        Assert.That(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Email && c.Value == user.Email), Is.True);
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
    /// Performs cleanup of test resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.postgresContainer.DisposeAsync();
        await this.dbContext.DisposeAsync();
    }
}