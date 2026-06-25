// <copyright file="EmailConfirmationRequestDto.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Models.DTOs;

/// <summary>
/// DTO for email confirmation request.
/// </summary>
public class EmailConfirmationRequestDto
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation link.
    /// </summary>
    public string ConfirmLink { get; set; } = string.Empty;
}