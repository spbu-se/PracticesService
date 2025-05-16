// <copyright file="Practice.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Models;

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
    /// Gets or sets ConsultantId column.
    /// </summary>
    public int? Consultantid { get; set; }

    /// <summary>
    /// Gets or sets SupervisorId column.
    /// </summary>
    public int Supervisorid { get; set; }

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
    /// Gets or sets virtual Consultant.
    /// </summary>
    public virtual Consultant Consultant { get; set; } = null!;

    /// <summary>
    /// Gets or sets virtual Supervisor.
    /// </summary>
    public virtual Lecturer Supervisor { get; set; } = null!;

    /// <summary>
    /// Gets or sets virtual Theme.
    /// </summary>
    public virtual Theme Theme { get; set; } = null!;
}
