// <copyright file="ApplicationUserDTO.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Application User DTO.
/// </summary>
public class ApplicationUserDTO
{
    /// <summary>
    /// Gets or sets user email.
    /// </summary>
    public required string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets user password.
    /// </summary>
    public required string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets FirstName column.
    /// </summary>
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets LastName column.
    /// </summary>
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets MiddleName column.
    /// </summary>
    [MaxLength(100)]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Gets or sets Roles.
    /// </summary>
    public string[]? Roles { get; set; }
}