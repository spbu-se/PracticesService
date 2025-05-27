// <copyright file="GoalsAndTasks.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents the goals and tasks associated with a practice.
/// </summary>
public class GoalsAndTasks
{
    /// <summary>
    /// Gets or sets the unique identifier of the goals and tasks record.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the practice identifier.
    /// </summary>
    public int PracticeId { get; set; }

    /// <summary>
    /// Gets or sets the goals description.
    /// </summary>
    public string Goals { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tasks description.
    /// </summary>
    public string Tasks { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}