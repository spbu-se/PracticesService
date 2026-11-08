// <copyright file="AuditEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event for audit logging.
/// </summary>
/// <param name="UserId">The ID of the user who performed the action.</param>
/// <param name="UserEmail">The email of the user who performed the action.</param>
/// <param name="UserRoles">The roles of the user.</param>
/// <param name="Action">The action performed (e.g., "PracticeCreated", "ThemeArchived").</param>
/// <param name="EntityType">The type of entity (e.g., "Practice", "Theme", "User").</param>
/// <param name="EntityId">The ID of the entity.</param>
/// <param name="Details">Additional details about the action.</param>
/// <param name="Status">The status of the action (Success/Failed).</param>
/// <param name="ErrorMessage">The error message if the action failed.</param>
/// <param name="Timestamp">The timestamp when the action occurred.</param>
/// <param name="SourceService">The service that performed the action.</param>
public record AuditEvent(
    string? UserId,
    string? UserEmail,
    string[]? UserRoles,
    string Action,
    string EntityType,
    string? EntityId,
    object? Details,
    string Status,
    string? ErrorMessage,
    DateTime Timestamp,
    string SourceService);