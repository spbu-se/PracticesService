// <copyright file="Theme.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Theme model.
/// </summary>
public partial class Theme
{
    /// <summary>
    /// Gets or sets Id column.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets Title column.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets Description column.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets Tags column.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets Level column.
    /// </summary>
    public string Level { get; set; } = null!;

    /// <summary>
    /// Gets or sets Department column.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether theme is archived.
    /// </summary>
    public bool Isarchived { get; set; }

    /// <summary>
    /// Gets or sets SuggestedBy column.
    /// </summary>
    public string Suggestedby { get; set; } = null!;

    /// <summary>
    /// Gets or sets ConsultantId column.
    /// </summary>
    public int Consultantid { get; set; }

    /// <summary>
    /// Gets or sets SupervisorId.
    /// </summary>
    public int Supervisorid { get; set; }

    /// <summary>
    /// Gets or sets CreatedDate column.
    /// </summary>
    public DateTime Createddate { get; set; }

    /// <summary>
    /// Gets or sets UpdatedDate column.
    /// </summary>
    public DateTime Updateddate { get; set; }

    /// <summary>
    /// Gets or sets virtual Practices.
    /// </summary>
    public virtual ICollection<Practice> Practices { get; set; } = new List<Practice>();

    /// <summary>
    /// Gets or sets virtual Supervisor.
    /// </summary>
    public virtual Lecturer Supervisor { get; set; } = null!;

    /// <summary>
    /// Gets or sets virtual Consultant.
    /// </summary>
    public virtual Consultant Consultant { get; set; } = null!;
}
