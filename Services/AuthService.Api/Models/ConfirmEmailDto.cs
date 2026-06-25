// <copyright file="ConfirmEmailDto.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models;

/// <summary>
/// DTO for email confirmation.
/// </summary>
public class ConfirmEmailDto
{
    /// <summary>
    /// Gets or sets the confirmation token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}