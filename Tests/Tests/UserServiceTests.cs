// <copyright file="UserServiceTests.cs" company="Your Company">
// Copyright (c) Your Company. All rights reserved.
// </copyright>

using AuthService.Api;
using AuthService.Api.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Tests;

/// <summary>
/// Unit tests for <see cref="UserService"/>.
/// </summary>
[TestFixture]
public class UserServiceTests
{
    private Mock<UserManager<ApplicationUser>> userManagerMock;
    private Mock<RoleManager<IdentityRole>> roleManagerMock;
    private UserService userService;

    /// <summary>
    /// Initializes test setup before each test method.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        this.userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        this.roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object, null, null, null, null);

        this.userService = new UserService(
            this.userManagerMock.Object, 
            this.roleManagerMock.Object);
    }

    /// <summary>
    /// Tests that RegisterUserAsync returns success when user is created.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterUserAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var dto = new ApplicationUserDTO
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "A",
        };

        this.userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var (result, user) = await this.userService.RegisterUserAsync(dto);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Email, Is.EqualTo(dto.Email));
    }

    /// <summary>
    /// Tests that AssignRolesAsync assigns roles when roles exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AssignRolesAsync_ShouldAssignRoles_WhenRolesExist()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        string[] roles = { "Admin", "User" };

        this.roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        this.userManagerMock.Setup(m => m.AddToRoleAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await this.userService.AssignRolesAsync(user, roles);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Is.EquivalentTo(roles));
    }

    /// <summary>
    /// Tests that UpdateUserAsync returns success when user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateUserAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = "123";
        var existingUser = new ApplicationUser { Id = userId, Email = "old@example.com" };

        var dto = new ApplicationUserDTO
        {
            Email = "new@example.com",
            FirstName = "New",
            LastName = "Name",
            MiddleName = "M",
            Password = "Password123!",
        };

        this.userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(existingUser);
        this.userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await this.userService.UpdateUserAsync(userId, dto);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(existingUser.Email, Is.EqualTo(dto.Email));
    }

    /// <summary>
    /// Tests that UpdateUserAsync fails when user doesn't exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UpdateUserAsync_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        this.userManagerMock.Setup(m => m.FindByIdAsync("not-found"))
            .ReturnsAsync((ApplicationUser)null);

        var dto = new ApplicationUserDTO
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await this.userService.UpdateUserAsync("not-found", dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.First().Description, Is.EqualTo("User not found"));
    }


    /// <summary>
    /// Tests that DeleteUserAsync returns success when user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteUserAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser { Id = "123" };

        this.userManagerMock.Setup(m => m.FindByIdAsync("123"))
            .ReturnsAsync(user);
        this.userManagerMock.Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await this.userService.DeleteUserAsync("123");

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    /// <summary>
    /// Tests that DeleteUserAsync fails when user is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteUserAsync_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        this.userManagerMock.Setup(m => m.FindByIdAsync("404"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await this.userService.DeleteUserAsync("404");

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.First().Description, Is.EqualTo("User not found"));
    }
}