// <copyright file="IAuditService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Audit;

using Contracts;

/// <summary>
/// Service for sending audit events.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an action.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="details">Additional details.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="userEmail">The user email.</param>
    /// <param name="userRoles">The user roles.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogActionAsync(
        string action,
        string entityType,
        string? entityId,
        object? details,
        string? userId = null,
        string? userEmail = null,
        string[]? userRoles = null);

    /// <summary>
    /// Logs an error.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="userEmail">The user email.</param>
    /// <param name="userRoles">The user roles.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogErrorAsync(
        string action,
        string entityType,
        string? entityId,
        string errorMessage,
        string? userId = null,
        string? userEmail = null,
        string[]? userRoles = null);
}