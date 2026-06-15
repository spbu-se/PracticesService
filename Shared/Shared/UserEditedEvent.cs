// <copyright file="UserEditedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event for editing a User.
/// </summary>
/// <param name="UserId">User id.</param>
/// <param name="Username">Username.</param>
/// <param name="FirstName">First Name.</param>
/// <param name="LastName">Last Name.</param>
/// <param name="MiddleName">Middle Name.</param>
/// <param name="RolesToAdd">Roles that need to add.</param>
/// <param name="RolesToRemove">Roles that need to remove.</param>
/// <param name="CurrentRoles">Current roles.</param>
/// <param name="CreatedAt">Date of creation.</param>
public record UserEditedEvent(
    string UserId,
    string Username,
    string FirstName,
    string LastName,
    string? MiddleName,
    IEnumerable<string>? RolesToAdd,
    IEnumerable<string>? RolesToRemove,
    IEnumerable<string>? CurrentRoles,
    DateTime CreatedAt);