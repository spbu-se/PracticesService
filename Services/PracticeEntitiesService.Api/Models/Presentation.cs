// <copyright file="Presentation.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents a presentation file associated with a practice.
/// </summary>
public class Presentation
{
    /// <summary>
    /// Gets or sets the unique identifier of the presentation.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the practice identifier this presentation belongs to.
    /// </summary>
    public int PracticeId { get; set; }

    /// <summary>
    /// Gets or sets the file name of the presentation.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the link to the presentation (e.g., cloud storage or document link).
    /// </summary>
    public string Link { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of comments related to this presentation.
    /// </summary>
    public List<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the version number of the presentation.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the presentation was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}