// <copyright file="ForgotPasswordDto.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for forgot password request.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}