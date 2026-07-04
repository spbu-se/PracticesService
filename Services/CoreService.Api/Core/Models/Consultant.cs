// <copyright file="Consultant.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Models;

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
    /// Gets or sets Email column.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Contact column.
    /// </summary>
    public string Contact { get; set; } = null!;

    /// <summary>
    /// Gets or sets UserId column.
    /// </summary>
    public string? Userid { get; set; }

    /// <summary>
    /// Gets or sets virtual Themes.
    /// </summary>
    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();

    /// <summary>
    /// Gets or sets virtual Practices.
    /// </summary>
    public virtual ICollection<Practice> Practices { get; set; } = new List<Practice>();
}
