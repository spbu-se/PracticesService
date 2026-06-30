// <copyright file="ResendConfirmationDto.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models;

/// <summary>
/// DTO for resending email confirmation.
/// </summary>
public class ResendConfirmationDto
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}