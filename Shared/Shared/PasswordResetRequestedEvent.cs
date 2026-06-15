// <copyright file="PasswordResetRequestedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event for password reset request.
/// </summary>
/// <param name="UserId">User identifier.</param>
/// <param name="Email">User email address.</param>
/// <param name="ResetLink">Password reset link.</param>
/// <param name="UserName">User name.</param>
/// <param name="RequestedAt">Date and time when the request was made.</param>
public record PasswordResetRequestedEvent(
    string UserId,
    string Email,
    string ResetLink,
    string UserName,
    DateTime RequestedAt);