// <copyright file="Practice.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Practice model.
/// </summary>
public partial class Practice
{
    /// <summary>
    /// Gets or sets Id column.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets StudentId column.
    /// </summary>
    public int Studentid { get; set; }

    /// <summary>
    /// Gets or sets ThemeId column.
    /// </summary>
    public int Themeid { get; set; }

    /// <summary>
    /// Gets or sets Type column.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets FinalGrade column.
    /// </summary>
    public string? Finalgrade { get; set; }

    /// <summary>
    /// Gets or sets Status column.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets CreatedDate column.
    /// </summary>
    public DateTime Createddate { get; set; }

    /// <summary>
    /// Gets or sets UpdatedDate column.
    /// </summary>
    public DateTime Updateddate { get; set; }

    /// <summary>
    /// Gets or sets virtual Student.
    /// </summary>
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets virtual Theme.
    /// </summary>
    public virtual Theme Theme { get; set; } = null!;
}
