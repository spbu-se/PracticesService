// <copyright file="TextWork.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents a text work document related to a practice.
/// </summary>
public class TextWork
{
    /// <summary>
    /// Gets or sets the unique identifier of the text work.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the practice this text work belongs to.
    /// </summary>
    public int PracticeId { get; set; }

    /// <summary>
    /// Gets or sets the file name of the text work.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of comments associated with this text work.
    /// </summary>
    public List<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the link to the text work (e.g., Google Docs).
    /// </summary>
    public string Link { get; set; } = null!;

    /// <summary>
    /// Gets or sets the version number of the text work.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the text work was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Gets or sets File.
    /// </summary>
    [BsonIgnore]
    public IFormFile? File { get; set; }
}