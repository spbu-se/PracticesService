// <copyright file="Report.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents a report related to a practice.
/// </summary>
public class Report
{
    /// <summary>
    /// Gets or sets the unique identifier of the report.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the practice identifier this report belongs to.
    /// </summary>
    public int PracticeId { get; set; }

    /// <summary>
    /// Gets or sets the done section of the report.
    /// </summary>
    public string Done { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the planned section of the report.
    /// </summary>
    public string Planned { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of comments associated with this report.
    /// </summary>
    public List<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the creation date and time of the report.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}