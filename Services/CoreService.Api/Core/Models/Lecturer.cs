// <copyright file="Lecturer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Lecturer model.
/// </summary>
public partial class Lecturer
{
    /// <summary>
    /// Gets or sets Id column.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets FirstName column.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets LastName column.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets MiddleName column.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Gets or sets UserId column.
    /// </summary>
    public string Userid { get; set; } = null!;

    /// <summary>
    /// Gets or sets Department column.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether can supervise VKR.
    /// </summary>
    public bool Cansupervisevkr { get; set; }

    /// <summary>
    /// Gets or sets virtual Themes.
    /// </summary>
    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();
}
