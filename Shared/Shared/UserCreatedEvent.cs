// <copyright file="UserCreatedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts
{
    /// <summary>
    /// Event for creating a User.
    /// </summary>
    /// <param name="UserId">User id.</param>
    /// <param name="Username">Username.</param>
    /// <param name="FirstName">First Name.</param>
    /// <param name="LastName">Last Name.</param>
    /// <param name="MiddleName">Middle Name.</param>
    /// <param name="Roles">Roles.</param>
    /// <param name="CreatedAt">Date of creation.</param>
    public record UserCreatedEvent(
    string UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? MiddleName,
    string[] Roles,
    DateTime CreatedAt);
}
