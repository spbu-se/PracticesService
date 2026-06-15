// <copyright file="PasswordResetRequestDTO.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Models.DTOs;

/// <summary>
/// Data transfer object for password reset request.
/// </summary>
public class PasswordResetRequestDto
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password reset link.
    /// </summary>
    public string ResetLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}