// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using System.Text;
using AuditService.Api.Consumers;
using AuditService.Api.Models;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
var gatewayBasePath = builder.Configuration["Swagger:GatewayBasePath"] ?? "/api";

builder.Services.AddSwaggerGen(c =>
{
    c.AddServer(new OpenApiServer
    {
        Url = gatewayBasePath,
        Description = "Gateway endpoint",
    });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter JWT token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
        });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                },
                new List<string>()
            },
        });
});

builder.Services.AddAuthentication("GatewayAuth")
    .AddScheme<AuthenticationSchemeOptions, GatewayAuthHandler.GatewayAuthHandler>("GatewayAuth", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Администратор"));
});

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "CorsPolicy",
            policyBuilder => policyBuilder
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed((_) => true)
                .AllowAnyHeader());
    });

// MongoDB
var connectionString = builder.Configuration.GetValue<string>("MongoSettings:ConnectionString")
    ?? "mongodb://admin:admin123@practice-entities.db:27017/admin";
var databaseName = builder.Configuration.GetValue<string>("MongoSettings:DatabaseName") ?? "audit_db";

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuditConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "admin123");
            h.Heartbeat(TimeSpan.FromSeconds(30));
            h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
        });

        cfg.ReceiveEndpoint("audit-events", e =>
        {
            e.UseMessageRetry(retry => retry.Intervals(1, 2, 5));
            e.PrefetchCount = 10;
            e.ConcurrentMessageLimit = 5;
            e.ConfigureConsumer<AuditConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var collection = db.GetCollection<AuditLog>("audit_logs");

        var indexKeys = Builders<AuditLog>.IndexKeys
            .Ascending(x => x.Action)
            .Descending(x => x.Timestamp);
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<AuditLog>(indexKeys),
            cancellationToken: CancellationToken.None);

        var userIndex = Builders<AuditLog>.IndexKeys.Ascending(x => x.UserEmail);
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<AuditLog>(userIndex),
            cancellationToken: CancellationToken.None);

        Console.WriteLine("Audit indexes created successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to create audit indexes: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/audit", async (
    IMongoDatabase db,
    [FromQuery] string? action,
    [FromQuery] string? userId,
    [FromQuery] string? userEmail,
    [FromQuery] string? entityType,
    [FromQuery] string? entityId,
    [FromQuery] string? status,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] int limit = 50,
    [FromQuery] int skip = 0) =>
{
    var collection = db.GetCollection<AuditLog>("audit_logs");

    var filterBuilder = Builders<AuditLog>.Filter;
    var filters = new List<FilterDefinition<AuditLog>>();

    if (!string.IsNullOrEmpty(action))
    {
        filters.Add(filterBuilder.Eq(x => x.Action, action));
    }

    if (!string.IsNullOrEmpty(userId))
    {
        filters.Add(filterBuilder.Eq(x => x.UserId, userId));
    }

    if (!string.IsNullOrEmpty(userEmail))
    {
        filters.Add(filterBuilder.Eq(x => x.UserEmail, userEmail));
    }

    if (!string.IsNullOrEmpty(entityType))
    {
        filters.Add(filterBuilder.Eq(x => x.EntityType, entityType));
    }

    if (!string.IsNullOrEmpty(entityId))
    {
        filters.Add(filterBuilder.Eq(x => x.EntityId, entityId));
    }

    if (!string.IsNullOrEmpty(status))
    {
        filters.Add(filterBuilder.Eq(x => x.Status, status));
    }

    if (from.HasValue)
    {
        filters.Add(filterBuilder.Gte(x => x.Timestamp, from.Value.ToUniversalTime()));
    }

    if (to.HasValue)
    {
        filters.Add(filterBuilder.Lte(x => x.Timestamp, to.Value.ToUniversalTime()));
    }

    var filter = filters.Any() ? filterBuilder.And(filters) : FilterDefinition<AuditLog>.Empty;

    var totalCount = await collection.CountDocumentsAsync(filter);

    var logs = await collection.Find(filter)
        .SortByDescending(x => x.Timestamp)
        .Skip(skip)
        .Limit(limit)
        .ToListAsync();

    var result = logs.Select(log => new
    {
        log.Id,
        log.UserId,
        log.UserEmail,
        log.UserRoles,
        log.Action,
        log.EntityType,
        log.EntityId,
        Details = log.Details != null ? BsonTypeMapper.MapToDotNetValue(log.Details) : null,
        log.Status,
        log.ErrorMessage,
        Timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
        log.SourceService,
    });

    return Results.Ok(new
    {
        Total = totalCount,
        Skip = skip,
        Limit = limit,
        Items = result,
    });
})
.WithName("GetAuditLogs")
.WithOpenApi()
.RequireAuthorization("AdminOnly");

app.MapGet("/api/audit/stats", async (
    IMongoDatabase db,
    [FromQuery] int days = 7,
    [FromQuery] string? action = null) =>
{
    var collection = db.GetCollection<AuditLog>("audit_logs");

    var since = DateTime.UtcNow.AddDays(-days);
    var filterBuilder = Builders<AuditLog>.Filter;
    var filters = new List<FilterDefinition<AuditLog>>();

    filters.Add(filterBuilder.Gte(x => x.Timestamp, since));

    if (!string.IsNullOrEmpty(action))
    {
        filters.Add(filterBuilder.Eq(x => x.Action, action));
    }

    var filter = filters.Any() ? filterBuilder.And(filters) : FilterDefinition<AuditLog>.Empty;

    var pipeline = new[]
    {
        new BsonDocument("$match", filter.ToBsonDocument()),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$Action" },
            { "count", new BsonDocument("$sum", 1) },
        }),
        new BsonDocument("$sort", new BsonDocument("count", -1)),
    };

    var result = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

    var stats = result.Select(doc => new
    {
        Action = doc["_id"].AsString,
        Count = doc["count"].AsInt32,
    });

    var total = stats.Sum(x => x.Count);

    return Results.Ok(new
    {
        Period = $"{days} days",
        Total = total,
        Stats = stats,
    });
})
.WithName("GetAuditStats")
.WithOpenApi()
.RequireAuthorization("AdminOnly");

app.MapGet("/api/audit/{id}", async (IMongoDatabase db, string id) =>
{
    var collection = db.GetCollection<AuditLog>("audit_logs");

    try
    {
        var objectId = new ObjectId(id);
        var filter = Builders<AuditLog>.Filter.Eq(x => x.Id, objectId.ToString());

        var log = await collection.Find(filter).FirstOrDefaultAsync();

        if (log == null)
        {
            return Results.NotFound($"Audit event with ID {id} not found");
        }

        var result = new
        {
            log.Id,
            log.UserId,
            log.UserEmail,
            log.UserRoles,
            log.Action,
            log.EntityType,
            log.EntityId,
            Details = log.Details != null ? BsonTypeMapper.MapToDotNetValue(log.Details) : null,
            log.Status,
            log.ErrorMessage,
            Timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            log.SourceService,
        };

        return Results.Ok(result);
    }
    catch (FormatException)
    {
        return Results.BadRequest($"Invalid ID format: {id}");
    }
})
.WithName("GetAuditEvent")
.WithOpenApi()
.RequireAuthorization("AdminOnly");

app.Run();