// <copyright file="Repository.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents a code repository related to a practice.
/// </summary>
public class Repository
{
    /// <summary>
    /// Gets or sets the unique identifier of the repository.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the practice this repository belongs to.
    /// </summary>
    public int PracticeId { get; set; }

    /// <summary>
    /// Gets or sets the link to the repository.
    /// </summary>
    public string RepositoryLink { get; set; } = null!;

    /// <summary>
    /// Gets or sets the account name, used for group projects.
    /// </summary>
    public string AccountName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the repository was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}