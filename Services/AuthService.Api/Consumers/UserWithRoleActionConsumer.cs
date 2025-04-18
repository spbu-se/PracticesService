// <copyright file="UserWithRoleActionConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Consumers;

using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Consumer for user with roles events.
/// </summary>
public class UserWithRoleActionConsumer : IConsumer<UserWithRoleActionEvent>
{
    private readonly AuthDbContext context;
    private readonly ILogger<UserWithRoleActionConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWithRoleActionConsumer"/> class.
    /// </summary>
    /// <param name="context">Auth DB context.</param>
    /// <param name="logger">Logger.</param>
    public UserWithRoleActionConsumer(
        AuthDbContext context,
        ILogger<UserWithRoleActionConsumer> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Consumes event.
    /// </summary>
    /// <param name="consumeContext">Consume event context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<UserWithRoleActionEvent> consumeContext)
    {
        try
        {
            const string targetRole = "Научный руководитель";

            if (consumeContext.Message.Role != targetRole)
            {
                this.logger.LogDebug("Skipping message for non-target role: {Role}", consumeContext.Message.Role);
                return;
            }

            switch (consumeContext.Message.Action)
            {
                case UserActionType.Delete:
                    await this.HandleRoleRemoval(consumeContext.Message.UserId, targetRole);
                    break;

                case UserActionType.Create:
                    await this.HandleRoleAssignment(consumeContext.Message.UserId, targetRole);
                    break;

                case UserActionType.Update:
                    await this.HandleUserUpdate(
                        consumeContext.Message.UserId,
                        consumeContext.Message.FirstName,
                        consumeContext.Message.LastName,
                        consumeContext.Message.MiddleName);
                    break;

                default:
                    this.logger.LogWarning("Unhandled action type: {Action}", consumeContext.Message.Action);
                    break;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing user role action for user {UserId}", consumeContext.Message.UserId);
            throw;
        }
    }

    private async Task HandleRoleAssignment(string userId, string roleName)
    {
        var role = await this.context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role == null)
        {
            this.logger.LogError("Role {RoleName} not found", roleName);
            throw new InvalidOperationException($"Role {roleName} not found");
        }

        var existingAssignment = await this.context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (existingAssignment)
        {
            this.logger.LogWarning("User {UserId} already has role {RoleName}", userId, roleName);
            return;
        }

        this.context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = userId,
            RoleId = role.Id,
        });

        await this.context.SaveChangesAsync();
        this.logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
    }

    private async Task HandleRoleRemoval(string userId, string roleName)
    {
        var role = await this.context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role == null)
        {
            this.logger.LogError("Role {RoleName} not found", roleName);
            throw new InvalidOperationException($"Role {roleName} not found");
        }

        var userRole = await this.context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (userRole == null)
        {
            this.logger.LogWarning("User {UserId} doesn't have role {RoleName}", userId, roleName);
            return;
        }

        this.context.UserRoles.Remove(userRole);
        await this.context.SaveChangesAsync();
        this.logger.LogInformation("Removed role {RoleName} from user {UserId}", roleName, userId);
    }

    private async Task HandleUserUpdate(
        string userId,
        string firstName,
        string lastName,
        string? middleName)
    {
        var user = await this.context.Users.FindAsync(userId);
        if (user == null)
        {
            this.logger.LogError("User {UserId} not found", userId);
            throw new InvalidOperationException($"User {userId} not found");
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.MiddleName = middleName;

        await this.context.SaveChangesAsync();
        this.logger.LogInformation("Updated user details for {UserId}", userId);
    }
}