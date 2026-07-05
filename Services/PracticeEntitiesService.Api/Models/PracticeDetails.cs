// <copyright file="PracticeDetails.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

/// <summary>
/// Practice details.
/// </summary>
public class PracticeDetails
{
    /// <summary>
    /// Gets or sets the practice ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the theme details.
    /// </summary>
    public ThemeDetails? Theme { get; set; }

    /// <summary>
    /// Gets or sets the student details.
    /// </summary>
    public StudentDetails? Student { get; set; }

    /// <summary>
    /// Gets or sets the supervisor details.
    /// </summary>
    public LecturerDetails? Supervisor { get; set; }
}
