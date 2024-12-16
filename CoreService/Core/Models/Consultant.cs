// <copyright file="Consultant.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Consultant table model.
/// </summary>
public partial class Consultant
{
    /// <summary>
    /// Gets or sets Id column.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets Name column.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets Contact column.
    /// </summary>
    public string Contact { get; set; } = null!;

    /// <summary>
    /// Gets or sets UserId column.
    /// </summary>
    public Guid? Userid { get; set; }

    /// <summary>
    /// Gets or sets virtual Themes.
    /// </summary>
    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();
}
