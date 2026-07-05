// <copyright file="AuditLog.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuditService.Api.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Audit log entry.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Gets or sets the audit log ID.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Gets or sets the user roles.
    /// </summary>
    public string[]? UserRoles { get; set; }

    /// <summary>
    /// Gets or sets the action performed.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity ID.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the additional details.
    /// </summary>
    public BsonDocument? Details { get; set; }

    /// <summary>
    /// Gets or sets the status of the action.
    /// </summary>
    public string Status { get; set; } = "Success";

    /// <summary>
    /// Gets or sets the error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the action.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source service name.
    /// </summary>
    public string SourceService { get; set; } = string.Empty;
}