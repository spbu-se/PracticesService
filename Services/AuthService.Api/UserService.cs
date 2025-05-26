// <copyright file="UserService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using Contracts;

namespace AuthService.Api;

using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Api.Models;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Service for managing user operations such as registration, update, deletion, and role assignment.
/// </summary>
public class UserService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userManager">User manager for identity operations.</param>
    /// <param name="roleManager">Role manager for role operations.</param>
    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
    }

    /// <summary>
    /// Registers a new user using the provided user DTO.
    /// </summary>
    /// <param name="userDto">The user DTO containing registration details.</param>
    /// <returns>
    /// A tuple containing the <see cref="IdentityResult"/> and the created <see cref="ApplicationUser"/>, or null if creation failed.
    /// </returns>
    public async Task<(IdentityResult Result, ApplicationUser? User)> RegisterUserAsync(ApplicationUserDTO userDto)
    {
        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            MiddleName = userDto.MiddleName,
        };

        var result = await this.userManager.CreateAsync(user, userDto.Password);

        return (result, result.Succeeded ? user : null);
    }

    /// <summary>
    /// Assigns roles to the specified user.
    /// </summary>
    /// <param name="user">The user to assign roles to.</param>
    /// <param name="roles">The roles to assign.</param>
    /// <returns>A list of successfully assigned role names.</returns>
    public async Task<List<string>> AssignRolesAsync(ApplicationUser user, string[]? roles)
    {
        var assignedRoles = new List<string>();

        if (!(roles?.Length > 0))
        {
            return assignedRoles;
        }

        foreach (var role in roles)
        {
            if (!await this.roleManager.RoleExistsAsync(role))
            {
                await this.roleManager.CreateAsync(new IdentityRole(role));
            }

            await this.userManager.AddToRoleAsync(user, role);
            assignedRoles.Add(role);
        }

        return assignedRoles;
    }

    /// <summary>
    /// Updates the specified user with the new data.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="dto">The DTO with updated user information.</param>
    /// <returns>The result of the update operation.</returns>
    public async Task<IdentityResult> UpdateUserAsync(string userId, UserDTO dto)
    {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.MiddleName = dto.MiddleName;
        user.Email = dto.Email;
        user.UserName = dto.Email;

        return await this.userManager.UpdateAsync(user);
    }

    /// <summary>
    /// Deletes the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        return await this.userManager.DeleteAsync(user);
    }
}
