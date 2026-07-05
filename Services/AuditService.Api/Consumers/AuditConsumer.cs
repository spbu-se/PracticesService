// <copyright file="AuditConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuditService.Api.Consumers;

using System.Text.Json;
using AuditService.Api.Models;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

/// <summary>
/// Consumer for audit events.
/// </summary>
public class AuditConsumer : IConsumer<AuditEvent>
{
    private readonly IMongoCollection<AuditLog> collection;
    private readonly ILogger<AuditConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditConsumer"/> class.
    /// </summary>
    /// <param name="database">MongoDB database.</param>
    /// <param name="logger">Logger instance.</param>
    public AuditConsumer(IMongoDatabase database, ILogger<AuditConsumer> logger)
    {
        this.collection = database.GetCollection<AuditLog>("audit_logs");
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AuditEvent> context)
    {
        var message = context.Message;

        this.logger.LogDebug(
            "Audit event: {Action}, Entity: {EntityType}:{EntityId}, Status: {Status}",
            message.Action,
            message.EntityType,
            message.EntityId,
            message.Status);

        try
        {
            BsonDocument? detailsBson = null;
            if (message.Details != null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(message.Details);
                    detailsBson = BsonDocument.Parse(json);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(ex, "Failed to serialize Details for audit event: {Action}", message.Action);
                    detailsBson = new BsonDocument { { "error", "Failed to serialize details" } };
                }
            }

            var log = new AuditLog
            {
                UserId = message.UserId,
                UserEmail = message.UserEmail,
                UserRoles = message.UserRoles,
                Action = message.Action,
                EntityType = message.EntityType,
                EntityId = message.EntityId,
                Details = detailsBson,
                Status = message.Status,
                ErrorMessage = message.ErrorMessage,
                Timestamp = message.Timestamp,
                SourceService = message.SourceService,
            };

            await this.collection.InsertOneAsync(log);

            this.logger.LogDebug(
                "Audit event saved: {Action} for {EntityType}:{EntityId}",
                message.Action,
                message.EntityType,
                message.EntityId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to save audit event: {Action}", message.Action);
            throw;
        }
    }
}