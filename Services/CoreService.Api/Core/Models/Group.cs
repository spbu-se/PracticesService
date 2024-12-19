// <copyright file="Group.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Group model.
/// </summary>
public partial class Group
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
    /// Gets or sets Program column.
    /// </summary>
    public string Program { get; set; } = null!;

    /// <summary>
    /// Gets or sets Year column.
    /// </summary>
    public short Year { get; set; }

    /// <summary>
    /// Gets or sets virtual Students.
    /// </summary>
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
