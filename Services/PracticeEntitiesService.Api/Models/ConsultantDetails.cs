// <copyright file="ConsultantDetails.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

/// <summary>
/// Consultant of a practice as the core service returns it.
/// </summary>
public class ConsultantDetails
{
    /// <summary>
    /// Gets or sets the user id of the consultant.
    /// </summary>
    public string? Userid { get; set; }
}
