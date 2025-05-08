// <copyright file="UserWithRoleActionEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event for actioning with role user tables.
/// </summary>
/// <param name="UserId">User id.</param>
/// <param name="FirstName">First Name.</param>
/// <param name="LastName">Last Name.</param>
/// <param name="MiddleName">Middle Name.</param>
/// <param name="Action">Action.</param>
/// <param name="Role">Role.</param>
/// <param name="CreatedAt">Date of creation.</param>
public record UserWithRoleActionEvent(
    string UserId,
    string FirstName,
    string LastName,
    string? MiddleName,
    UserActionType Action,
    string Role,
    DateTime CreatedAt);