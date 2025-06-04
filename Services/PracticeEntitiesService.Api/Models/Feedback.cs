// <copyright file="Feedback.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents feedback associated with a practice report.
/// </summary>
public class Feedback
{
    /// <summary>
    /// Gets or sets the unique identifier of the feedback.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the associated practice identifier.
    /// </summary>
    public int PracticeId { get; set; }

    /// <summary>
    /// Gets or sets the name of the feedback file.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the link to the feedback. Can be null.
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the feedback was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the type of feedback, e.g., "Supervisor" or "Consultant".
    /// </summary>
    public string FeedbackType { get; set; } = null!;

    /// <summary>
    /// Gets or sets File.
    /// </summary>
    [BsonIgnore]
    public IFormFile? File { get; set; }
}