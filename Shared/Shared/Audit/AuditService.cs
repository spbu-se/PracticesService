// <copyright file="AuditService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Audit;

using System.Reflection;
using System.Security.Claims;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for sending audit events.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IPublishEndpoint publishEndpoint;
    private readonly ILogger<AuditService> logger;
    private readonly string? sourceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="publishEndpoint">MassTransit publish endpoint.</param>
    /// <param name="logger">Logger instance.</param>
    public AuditService(
        IPublishEndpoint publishEndpoint,
        ILogger<AuditService> logger)
    {
        this.publishEndpoint = publishEndpoint;
        this.logger = logger;
        this.sourceService = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
    }

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
    public async Task LogActionAsync(
        string action,
        string entityType,
        string? entityId,
        object? details,
        string? userId = null,
        string? userEmail = null,
        string[]? userRoles = null)
    {
        try
        {
            await this.publishEndpoint.Publish(new AuditEvent(
                UserId: userId,
                UserEmail: userEmail,
                UserRoles: userRoles,
                Action: action,
                EntityType: entityType,
                EntityId: entityId,
                Details: details,
                Status: "Success",
                ErrorMessage: null,
                Timestamp: DateTime.UtcNow,
                SourceService: this.sourceService));

            this.logger.LogDebug("Audit event sent: {Action} for {EntityType}:{EntityId}", action, entityType, entityId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send audit event: {Action}", action);
        }
    }

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
    public async Task LogErrorAsync(
        string action,
        string entityType,
        string? entityId,
        string errorMessage,
        string? userId = null,
        string? userEmail = null,
        string[]? userRoles = null)
    {
        try
        {
            await this.publishEndpoint.Publish(new AuditEvent(
                UserId: userId,
                UserEmail: userEmail,
                UserRoles: userRoles,
                Action: action,
                EntityType: entityType,
                EntityId: entityId,
                Details: null,
                Status: "Failed",
                ErrorMessage: errorMessage,
                Timestamp: DateTime.UtcNow,
                SourceService: this.sourceService));

            this.logger.LogDebug("Audit error event sent: {Action} for {EntityType}:{EntityId}", action, entityType, entityId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send audit error event: {Action}", action);
        }
    }

    /// <summary>
    /// Logs an action with current user from HttpContext.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="details">Additional details.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LogActionWithUserAsync(
        string action,
        string entityType,
        string? entityId,
        object? details,
        HttpContext? httpContext = null)
    {
        var (userId, userEmail, userRoles) = this.GetUserInfo(httpContext);
        await this.LogActionAsync(action, entityType, entityId, details, userId, userEmail, userRoles);
    }

    /// <summary>
    /// Logs an error with current user from HttpContext.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LogErrorWithUserAsync(
        string action,
        string entityType,
        string? entityId,
        string errorMessage,
        HttpContext? httpContext = null)
    {
        var (userId, userEmail, userRoles) = this.GetUserInfo(httpContext);
        await this.LogErrorAsync(action, entityType, entityId, errorMessage, userId, userEmail, userRoles);
    }

    /// <summary>
    /// Gets user information from HttpContext.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A tuple containing user ID, email and roles.</returns>
    private (string? UserId, string? UserEmail, string[]? UserRoles) GetUserInfo(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return (null, null, null);
        }

        var user = httpContext.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return (userId, userEmail, userRoles.Length > 0 ? userRoles : null);
    }
}