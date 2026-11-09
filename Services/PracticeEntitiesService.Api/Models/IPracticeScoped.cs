// <copyright file="IPracticeScoped.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

/// <summary>
/// Data that belongs to a practice, access to it follows the access to the practice.
/// </summary>
public interface IPracticeScoped
{
    /// <summary>
    /// Gets the id of the practice the data belongs to.
    /// </summary>
    int PracticeId { get; }
}
