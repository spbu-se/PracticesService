// <copyright file="ApplicationUserDTO.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace CoreService.Core.Models;

/// <summary>
/// Application User DTO.
/// </summary>
public class ApplicationUserDTO
{
    /// <summary>
    /// Gets or sets Email column.
    /// </summary>
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets UserName column.
    /// </summary>
    [MaxLength(100)]
    public string UserName { get; set; } = null!;

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