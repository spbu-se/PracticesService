// <copyright file="EmailConfirmationRequestedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when an email confirmation is requested.
/// </summary>
/// <param name="UserId">The ID of the user requesting confirmation.</param>
/// <param name="Email">The email address to confirm.</param>
/// <param name="UserName">The username of the user.</param>
/// <param name="ConfirmLink">The confirmation link to send.</param>
/// <param name="RequestedAt">The timestamp when the confirmation was requested.</param>
public record EmailConfirmationRequestedEvent(
    string UserId,
    string Email,
    string UserName,
    string ConfirmLink,
    DateTime RequestedAt);