// <copyright file="StudentDetails.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

/// <summary>
/// Student details.
/// </summary>
public class StudentDetails
{
    /// <summary>
    /// Gets or sets the student ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the middle name.
    /// </summary>
    public string? MiddleName { get; set; }
}