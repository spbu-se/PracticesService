// <copyright file="ApplicationUser.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application User model.
/// </summary>
public class ApplicationUser : IdentityUser
{
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
}
