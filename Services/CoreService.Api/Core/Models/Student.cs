// <copyright file="Student.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Student model.
/// </summary>
public partial class Student
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
    /// Gets or sets GroupId.
    /// </summary>
    public int Groupid { get; set; }

    /// <summary>
    /// Gets or sets virtual Group.
    /// </summary>
    public virtual Group Group { get; set; } = null!;

    /// <summary>
    /// Gets or sets virtual Practices.
    /// </summary>
    public virtual ICollection<Practice> Practices { get; set; } = new List<Practice>();
}
