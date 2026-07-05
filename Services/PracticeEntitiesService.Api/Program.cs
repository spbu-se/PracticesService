// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using Amazon.S3;
using Amazon.S3.Model;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PracticeEntities.Models;
using PracticeEntities.Services;
using Shared.Audit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = builder.Configuration["S3Storage:ServiceURL"],
        ForcePathStyle = true,
        AuthenticationRegion = builder.Configuration["S3Storage:Region"],
    };

    return new AmazonS3Client(
        builder.Configuration["S3Storage:AccessKey"],
        builder.Configuration["S3Storage:SecretKey"],
        config);
});

var mongoConnectionString = builder.Configuration.GetValue<string>("MongoSettings:ConnectionString")
    ?? "mongodb://admin:admin123@practice-entities.db:27017/admin";

var mongoDatabaseName = builder.Configuration.GetValue<string>("MongoSettings:DatabaseName") ?? "practice_entities";

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));

builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("CoreService", client =>
{
    client.BaseAddress = new Uri("http://core.api:8080/");
});

// Add Audit Service
builder.Services.AddAuditService();

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

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
            h.Heartbeat(TimeSpan.FromSeconds(30));
            h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
        });

        cfg.Message<GoalsAndTasksUpdatedEvent>(x => x.SetEntityName("goals-tasks-events"));
        cfg.Message<FeedbackSubmittedEvent>(x => x.SetEntityName("feedback-events"));
        cfg.Message<TextWorkSubmittedEvent>(x => x.SetEntityName("textwork-events"));
        cfg.Message<TextWorkCommentAddedEvent>(x => x.SetEntityName("textwork-comment-events"));
        cfg.Message<PresentationSubmittedEvent>(x => x.SetEntityName("presentation-events"));
        cfg.Message<PresentationCommentAddedEvent>(x => x.SetEntityName("presentation-comment-events"));
        cfg.Message<ReportSubmittedEvent>(x => x.SetEntityName("report-events"));
        cfg.Message<ReportCommentAddedEvent>(x => x.SetEntityName("report-comment-events"));
        cfg.Message<MessageSentEvent>(x => x.SetEntityName("message-events"));
    });
});

builder.Services.AddScoped<PracticeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/feedbacks", async (
    [FromForm] Feedback feedback,
    IAmazonS3 s3Client,
    IMongoDatabase db,
    IConfiguration config,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var bucketName = config["S3Storage:BucketName"];
        string? fileUrl = null;

        if (feedback.File != null)
        {
            var key = $"feedbacks/{feedback.PracticeId}/{feedback.FeedbackType}/{feedback.File.FileName}";
            using var stream = feedback.File.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = feedback.File.ContentType,
            };

            await s3Client.PutObjectAsync(putRequest);
            fileUrl = $"{config["S3Storage:ServiceURL"]}/{bucketName}/{key}";

            feedback.FileName = feedback.File.FileName;
            feedback.Link = fileUrl;
        }
        else
        {
            feedback.FileName = "(no file)";
        }

        feedback.UploadedAt = DateTime.UtcNow;

        var collection = db.GetCollection<Feedback>("feedbacks");
        await collection.InsertOneAsync(feedback);

        var practice = await practiceService.GetPracticeAsync(feedback.PracticeId);
        var studentEmail = practice?.Student?.Email;

        if (!string.IsNullOrEmpty(studentEmail))
        {
            await publishEndpoint.Publish(new FeedbackSubmittedEvent(
                PracticeId: feedback.PracticeId,
                PracticeTitle: practice?.Theme?.Title ?? "Practice",
                StudentEmail: studentEmail,
                FeedbackType: feedback.FeedbackType,
                FileName: feedback.FileName,
                SubmittedAt: feedback.UploadedAt));

            await auditService.LogActionAsync(
                "SubmitFeedback",
                "Feedback",
                feedback.Id,
                new { feedback.PracticeId, feedback.FeedbackType, StudentEmail = studentEmail });

            logger.LogInformation(
                "Feedback submitted for practice {PracticeId}. Notification sent to student: {StudentEmail}",
                feedback.PracticeId,
                studentEmail);
        }
        else
        {
            await auditService.LogErrorAsync(
                "SubmitFeedback",
                "Feedback",
                feedback.Id,
                "No student email found");
        }

        return Results.Ok(feedback);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "SubmitFeedback",
            "Feedback",
            null,
            ex.Message);

        logger.LogError(ex, "Error submitting feedback for practice {PracticeId}", feedback.PracticeId);
        return Results.Problem("Failed to submit feedback");
    }
}).DisableAntiforgery();

app.MapGet("/api/feedbacks/{practiceId:int}", async (
    int practiceId,
    [FromQuery] string? type,
    IMongoDatabase db,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Feedback>("feedbacks");

    var filterBuilder = Builders<Feedback>.Filter;
    var filter = filterBuilder.Eq(f => f.PracticeId, practiceId);

    if (!string.IsNullOrEmpty(type))
    {
        filter &= filterBuilder.Eq(f => f.FeedbackType, type);
    }

    var feedbacks = await collection.Find(filter)
        .SortByDescending(f => f.UploadedAt)
        .ToListAsync();

    await auditService.LogActionAsync(
        "GetFeedbacks",
        "Feedback",
        null,
        new { PracticeId = practiceId, Count = feedbacks.Count });

    return Results.Ok(feedbacks);
});

app.MapPost("/api/text-works", async (
    [FromForm] TextWork model,
    IAmazonS3 s3Client,
    IMongoDatabase db,
    IConfiguration config,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var bucketName = config["S3Storage:BucketName"];
        string? fileUrl = null;

        if (model.File != null)
        {
            var key = $"textworks/{model.PracticeId}/v{model.Version}/{model.File.FileName}";
            using var stream = model.File.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = model.File.ContentType,
            };

            await s3Client.PutObjectAsync(putRequest);
            fileUrl = $"{config["S3Storage:ServiceURL"]}/{bucketName}/{key}";
        }

        var textWork = new TextWork
        {
            PracticeId = model.PracticeId,
            FileName = model.File?.FileName ?? model.Link.Split('/').LastOrDefault() ?? string.Empty,
            Link = fileUrl ?? model.Link ?? string.Empty,
            Version = model.Version,
            UploadedAt = DateTime.UtcNow,
        };

        var collection = db.GetCollection<TextWork>("textworks");
        await collection.InsertOneAsync(textWork);

        var practice = await practiceService.GetPracticeAsync(model.PracticeId);
        var supervisorEmail = practice?.Supervisor?.Email;
        var studentEmail = practice?.Student?.Email;

        if (!string.IsNullOrEmpty(supervisorEmail))
        {
            await publishEndpoint.Publish(new TextWorkSubmittedEvent(
                PracticeId: textWork.PracticeId,
                PracticeTitle: practice?.Theme?.Title ?? "Practice",
                StudentEmail: studentEmail ?? "unknown",
                SupervisorEmail: supervisorEmail,
                FileName: textWork.FileName,
                Version: textWork.Version,
                SubmittedAt: textWork.UploadedAt));

            await auditService.LogActionAsync(
                "SubmitTextWork",
                "TextWork",
                textWork.Id,
                new { textWork.PracticeId, textWork.Version, SupervisorEmail = supervisorEmail });

            logger.LogInformation(
                "Text work submitted for practice {PracticeId}. Notification sent to supervisor: {SupervisorEmail}",
                textWork.PracticeId,
                supervisorEmail);
        }

        return Results.Ok(textWork);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "SubmitTextWork",
            "TextWork",
            null,
            ex.Message);

        logger.LogError(ex, "Error submitting text work for practice {PracticeId}", model.PracticeId);
        return Results.Problem("Failed to submit text work");
    }
}).DisableAntiforgery();

app.MapGet("/api/text-works/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<TextWork>("textworks");

    var filter = Builders<TextWork>.Filter.Eq(t => t.PracticeId, practiceId);
    var textWorks = await collection.Find(filter)
        .SortByDescending(t => t.Version)
        .ToListAsync();

    await auditService.LogActionAsync(
        "GetTextWorks",
        "TextWork",
        null,
        new { PracticeId = practiceId, Count = textWorks.Count });

    return Results.Ok(textWorks);
});

app.MapGet("/api/text-works/latest/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<TextWork>("textworks");

    var filter = Builders<TextWork>.Filter.Eq(t => t.PracticeId, practiceId);
    var latest = await collection.Find(filter)
        .SortByDescending(t => t.Version)
        .FirstOrDefaultAsync();

    await auditService.LogActionAsync(
        "GetLatestTextWork",
        "TextWork",
        null,
        new { PracticeId = practiceId });

    return latest is not null ? Results.Ok(latest) : Results.NotFound();
});

app.MapPost("/api/text-works/{id}/comments", async (
    IMongoDatabase db,
    string id,
    Comment input,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var collection = db.GetCollection<TextWork>("textworks");
        var filter = Builders<TextWork>.Filter.Eq(t => t.Id, id);
        var update = Builders<TextWork>.Update.Push(t => t.Comments, input);

        var result = await collection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            await auditService.LogErrorAsync(
                "AddTextWorkComment",
                "TextWork",
                id,
                "Text work not found");

            return Results.NotFound($"Text work with ID {id} not found");
        }

        var textWork = await collection.Find(t => t.Id == id).FirstOrDefaultAsync();

        if (textWork != null)
        {
            var practice = await practiceService.GetPracticeAsync(textWork.PracticeId);
            var studentEmail = practice?.Student?.Email;
            var supervisorEmail = practice?.Supervisor?.Email;

            if (!string.IsNullOrEmpty(studentEmail) || !string.IsNullOrEmpty(supervisorEmail))
            {
                await publishEndpoint.Publish(new TextWorkCommentAddedEvent(
                    PracticeId: textWork.PracticeId,
                    PracticeTitle: practice?.Theme?.Title ?? "Practice",
                    TextWorkId: id,
                    FileName: textWork.FileName,
                    Version: textWork.Version,
                    Author: input.Author,
                    CommentText: input.Text,
                    StudentEmail: studentEmail,
                    SupervisorEmail: supervisorEmail,
                    CreatedAt: input.CreatedAt));

                await auditService.LogActionAsync(
                    "AddTextWorkComment",
                    "TextWork",
                    id,
                    new { Author = input.Author });

                logger.LogInformation(
                    "Comment added to text work {TextWorkId} for practice {PracticeId}. Author: {Author}",
                    id,
                    textWork.PracticeId,
                    input.Author);
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "AddTextWorkComment",
            "TextWork",
            id,
            ex.Message);

        logger.LogError(ex, "Error adding comment to text work {TextWorkId}", id);
        return Results.Problem("Failed to add comment");
    }
});

app.MapPost("/api/presentations", async (
    [FromForm] Presentation model,
    IAmazonS3 s3Client,
    IMongoDatabase db,
    IConfiguration config,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var bucketName = config["S3Storage:BucketName"];
        string? fileUrl = null;

        if (model.File != null)
        {
            var key = $"presentations/{model.PracticeId}/v{model.Version}/{model.File.FileName}";
            using var stream = model.File.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = model.File.ContentType,
            };

            await s3Client.PutObjectAsync(putRequest);
            fileUrl = $"{config["S3Storage:ServiceURL"]}/{bucketName}/{key}";
        }

        var presentation = new Presentation
        {
            PracticeId = model.PracticeId,
            FileName = model.File?.FileName ?? model.Link.Split('/').LastOrDefault() ?? string.Empty,
            Link = fileUrl ?? model.Link ?? string.Empty,
            Version = model.Version,
            UploadedAt = DateTime.UtcNow,
        };

        var collection = db.GetCollection<Presentation>("presentations");
        await collection.InsertOneAsync(presentation);

        var practice = await practiceService.GetPracticeAsync(model.PracticeId);
        var supervisorEmail = practice?.Supervisor?.Email;
        var studentEmail = practice?.Student?.Email;

        if (!string.IsNullOrEmpty(supervisorEmail))
        {
            await publishEndpoint.Publish(new PresentationSubmittedEvent(
                PracticeId: presentation.PracticeId,
                PracticeTitle: practice?.Theme?.Title ?? "Practice",
                StudentEmail: studentEmail ?? "unknown",
                SupervisorEmail: supervisorEmail,
                FileName: presentation.FileName,
                Version: presentation.Version,
                SubmittedAt: presentation.UploadedAt));

            await auditService.LogActionAsync(
                "SubmitPresentation",
                "Presentation",
                presentation.Id,
                new { presentation.PracticeId, presentation.Version, SupervisorEmail = supervisorEmail });

            logger.LogInformation(
                "Presentation submitted for practice {PracticeId}. Notification sent to supervisor: {SupervisorEmail}",
                presentation.PracticeId,
                supervisorEmail);
        }

        return Results.Ok(presentation);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "SubmitPresentation",
            "Presentation",
            null,
            ex.Message);

        logger.LogError(ex, "Error submitting presentation for practice {PracticeId}", model.PracticeId);
        return Results.Problem("Failed to submit presentation");
    }
}).DisableAntiforgery();

app.MapGet("/api/presentations/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Presentation>("presentations");

    var filter = Builders<Presentation>.Filter.Eq(p => p.PracticeId, practiceId);
    var presentations = await collection.Find(filter)
        .SortByDescending(p => p.Version)
        .ToListAsync();

    await auditService.LogActionAsync(
        "GetPresentations",
        "Presentation",
        null,
        new { PracticeId = practiceId, Count = presentations.Count });

    return Results.Ok(presentations);
});

app.MapGet("/api/presentations/latest/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Presentation>("presentations");

    var filter = Builders<Presentation>.Filter.Eq(t => t.PracticeId, practiceId);
    var latest = await collection.Find(filter)
        .SortByDescending(t => t.Version)
        .FirstOrDefaultAsync();

    await auditService.LogActionAsync(
        "GetLatestPresentation",
        "Presentation",
        null,
        new { PracticeId = practiceId });

    return latest is not null ? Results.Ok(latest) : Results.NotFound();
});

app.MapPost("/api/presentations/{id}/comments", async (
    IMongoDatabase db,
    string id,
    Comment input,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var collection = db.GetCollection<Presentation>("presentations");
        var filter = Builders<Presentation>.Filter.Eq(t => t.Id, id);
        var update = Builders<Presentation>.Update.Push(t => t.Comments, input);

        var result = await collection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            await auditService.LogErrorAsync(
                "AddPresentationComment",
                "Presentation",
                id,
                "Presentation not found");

            return Results.NotFound($"Presentation with ID {id} not found");
        }

        var presentation = await collection.Find(t => t.Id == id).FirstOrDefaultAsync();

        if (presentation != null)
        {
            var practice = await practiceService.GetPracticeAsync(presentation.PracticeId);
            var studentEmail = practice?.Student?.Email;
            var supervisorEmail = practice?.Supervisor?.Email;

            if (!string.IsNullOrEmpty(studentEmail) || !string.IsNullOrEmpty(supervisorEmail))
            {
                await publishEndpoint.Publish(new PresentationCommentAddedEvent(
                    PracticeId: presentation.PracticeId,
                    PracticeTitle: practice?.Theme?.Title ?? "Practice",
                    PresentationId: id,
                    FileName: presentation.FileName,
                    Version: presentation.Version,
                    Author: input.Author,
                    CommentText: input.Text,
                    StudentEmail: studentEmail,
                    SupervisorEmail: supervisorEmail,
                    CreatedAt: input.CreatedAt));

                await auditService.LogActionAsync(
                    "AddPresentationComment",
                    "Presentation",
                    id,
                    new { Author = input.Author });

                logger.LogInformation(
                    "Comment added to presentation {PresentationId} for practice {PracticeId}. Author: {Author}",
                    id,
                    presentation.PracticeId,
                    input.Author);
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "AddPresentationComment",
            "Presentation",
            id,
            ex.Message);

        logger.LogError(ex, "Error adding comment to presentation {PresentationId}", id);
        return Results.Problem("Failed to add comment");
    }
});

app.MapPost("/api/goals-tasks", async (
    [FromBody] GoalsAndTasks input,
    IMongoDatabase db,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var collection = db.GetCollection<GoalsAndTasks>("goals_tasks");

        var existing = await collection.Find(x => x.PracticeId == input.PracticeId).FirstOrDefaultAsync();

        var practice = await practiceService.GetPracticeAsync(input.PracticeId);

        if (practice == null)
        {
            logger.LogWarning("Practice {PracticeId} not found", input.PracticeId);
            await auditService.LogErrorAsync(
                "SaveGoalsTasks",
                "GoalsTasks",
                null,
                $"Practice {input.PracticeId} not found");
            return Results.NotFound($"Practice with ID {input.PracticeId} not found");
        }

        var studentEmail = practice.Student?.Email ?? "student@example.com";
        var supervisorEmail = practice.Supervisor?.Email;
        var practiceTitle = practice.Theme?.Title ?? "Practice";

        if (existing != null)
        {
            existing.Goals = input.Goals;
            existing.Tasks = input.Tasks;
            existing.CreatedAt = DateTime.UtcNow;
            await collection.ReplaceOneAsync(x => x.Id == existing.Id, existing);

            await publishEndpoint.Publish(new GoalsAndTasksUpdatedEvent(
                PracticeId: input.PracticeId,
                PracticeTitle: practiceTitle,
                StudentEmail: studentEmail,
                SupervisorEmail: supervisorEmail,
                UpdatedAt: DateTime.UtcNow));

            await auditService.LogActionAsync(
                "UpdateGoalsTasks",
                "GoalsTasks",
                existing.Id,
                new { input.PracticeId, Action = "Update" });

            logger.LogInformation("Goals and tasks updated for practice {PracticeId}", input.PracticeId);
            return Results.Ok(existing);
        }

        input.Id = null;
        await collection.InsertOneAsync(input);

        await publishEndpoint.Publish(new GoalsAndTasksUpdatedEvent(
            PracticeId: input.PracticeId,
            PracticeTitle: practiceTitle,
            StudentEmail: studentEmail,
            SupervisorEmail: supervisorEmail,
            UpdatedAt: DateTime.UtcNow));

        await auditService.LogActionAsync(
            "CreateGoalsTasks",
            "GoalsTasks",
            input.Id,
            new { input.PracticeId, Action = "Create" });

        logger.LogInformation("Goals and tasks created for practice {PracticeId}", input.PracticeId);
        return Results.Created($"/api/goals-tasks/{input.PracticeId}", input);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "SaveGoalsTasks",
            "GoalsTasks",
            null,
            ex.Message);

        logger.LogError(ex, "Error saving goals and tasks for practice {PracticeId}", input.PracticeId);
        return Results.Problem("Failed to save goals and tasks");
    }
});

app.MapGet("/api/goals-tasks/{practiceId:int}", async (
    IMongoDatabase db,
    int practiceId,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<GoalsAndTasks>("goals_tasks");

    var result = await collection.Find(x => x.PracticeId == practiceId).FirstOrDefaultAsync();

    await auditService.LogActionAsync(
        "GetGoalsTasks",
        "GoalsTasks",
        null,
        new { PracticeId = practiceId, Found = result != null });

    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/api/reports", async (
    IMongoDatabase db,
    Report input,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var collection = db.GetCollection<Report>("reports");
        input.Id = null;
        input.CreatedAt = DateTime.UtcNow;
        await collection.InsertOneAsync(input);

        var practice = await practiceService.GetPracticeAsync(input.PracticeId);
        var studentEmail = practice?.Student?.Email;
        var supervisorEmail = practice?.Supervisor?.Email;

        if (!string.IsNullOrEmpty(studentEmail) || !string.IsNullOrEmpty(supervisorEmail))
        {
            await publishEndpoint.Publish(new ReportSubmittedEvent(
                PracticeId: input.PracticeId,
                PracticeTitle: practice?.Theme?.Title ?? "Practice",
                StudentEmail: studentEmail,
                SupervisorEmail: supervisorEmail,
                SubmittedAt: input.CreatedAt));

            await auditService.LogActionAsync(
                "CreateReport",
                "Report",
                input.Id,
                new { input.PracticeId });

            logger.LogInformation(
                "Report submitted for practice {PracticeId}. Notifications sent.",
                input.PracticeId);
        }

        return Results.Created($"/api/reports/{input.PracticeId}", input);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "CreateReport",
            "Report",
            null,
            ex.Message);

        logger.LogError(ex, "Error submitting report for practice {PracticeId}", input.PracticeId);
        return Results.Problem("Failed to submit report");
    }
});

app.MapGet("/api/reports/{practiceId:int}", async (
    IMongoDatabase db,
    int practiceId,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Report>("reports");
    var reports = await collection
        .Find(r => r.PracticeId == practiceId)
        .SortByDescending(r => r.CreatedAt)
        .ToListAsync();

    await auditService.LogActionAsync(
        "GetReports",
        "Report",
        null,
        new { PracticeId = practiceId, Count = reports.Count });

    return Results.Ok(reports);
});

app.MapPost("/api/reports/{id}/comments", async (
    IMongoDatabase db,
    string id,
    Comment input,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var collection = db.GetCollection<Report>("reports");
        var filter = Builders<Report>.Filter.Eq(r => r.Id, id);
        var update = Builders<Report>.Update.Push(r => r.Comments, input);

        var result = await collection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            await auditService.LogErrorAsync(
                "AddReportComment",
                "Report",
                id,
                "Report not found");

            return Results.NotFound($"Report with ID {id} not found");
        }

        var report = await collection.Find(r => r.Id == id).FirstOrDefaultAsync();

        if (report != null)
        {
            var practice = await practiceService.GetPracticeAsync(report.PracticeId);
            var studentEmail = practice?.Student?.Email;
            var supervisorEmail = practice?.Supervisor?.Email;

            if (!string.IsNullOrEmpty(studentEmail) || !string.IsNullOrEmpty(supervisorEmail))
            {
                await publishEndpoint.Publish(new ReportCommentAddedEvent(
                    PracticeId: report.PracticeId,
                    PracticeTitle: practice?.Theme?.Title ?? "Practice",
                    ReportId: id,
                    Author: input.Author,
                    CommentText: input.Text,
                    StudentEmail: studentEmail,
                    SupervisorEmail: supervisorEmail,
                    CreatedAt: input.CreatedAt));

                await auditService.LogActionAsync(
                    "AddReportComment",
                    "Report",
                    id,
                    new { Author = input.Author });

                logger.LogInformation(
                    "Comment added to report {ReportId} for practice {PracticeId}. Author: {Author}",
                    id,
                    report.PracticeId,
                    input.Author);
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "AddReportComment",
            "Report",
            id,
            ex.Message);

        logger.LogError(ex, "Error adding comment to report {ReportId}", id);
        return Results.Problem("Failed to add comment");
    }
});

app.MapPost("/api/repositories", async (
    IMongoDatabase db,
    Repository input,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Repository>("repositories");

    var existing = await collection.Find(r => r.PracticeId == input.PracticeId).FirstOrDefaultAsync();

    if (existing != null)
    {
        existing.RepositoryLink = input.RepositoryLink;
        existing.AccountName = input.AccountName;
        existing.UploadedAt = DateTime.UtcNow;

        await collection.ReplaceOneAsync(r => r.Id == existing.Id, existing);

        await auditService.LogActionAsync(
            "UpdateRepository",
            "Repository",
            existing.Id,
            new { input.PracticeId });

        return Results.Ok(existing);
    }

    input.Id = null;
    input.UploadedAt = DateTime.UtcNow;

    await collection.InsertOneAsync(input);

    await auditService.LogActionAsync(
        "CreateRepository",
        "Repository",
        input.Id,
        new { input.PracticeId });

    return Results.Created($"/api/repositories/{input.PracticeId}", input);
});

app.MapGet("/api/repositories/{practiceId:int}", async (
    IMongoDatabase db,
    int practiceId,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Repository>("repositories");
    var repo = await collection.Find(r => r.PracticeId == practiceId).FirstOrDefaultAsync();

    await auditService.LogActionAsync(
        "GetRepository",
        "Repository",
        null,
        new { PracticeId = practiceId, Found = repo != null });

    return repo != null ? Results.Ok(repo) : Results.NotFound();
});

app.MapPost("/api/messages", async (
    IMongoDatabase db,
    Message input,
    IPublishEndpoint publishEndpoint,
    PracticeService practiceService,
    ILogger<Program> logger,
    IAuditService auditService) =>
{
    try
    {
        var collection = db.GetCollection<Message>("messages");

        if (input.CreatedAt == default)
        {
            input.CreatedAt = DateTime.UtcNow;
        }

        await collection.InsertOneAsync(input);

        var practice = await practiceService.GetPracticeAsync(input.PracticeId);
        var studentEmail = practice?.Student?.Email;
        var supervisorEmail = practice?.Supervisor?.Email;

        if (!string.IsNullOrEmpty(studentEmail) || !string.IsNullOrEmpty(supervisorEmail))
        {
            await publishEndpoint.Publish(new MessageSentEvent(
                PracticeId: input.PracticeId,
                PracticeTitle: practice?.Theme?.Title ?? "Practice",
                StudentEmail: studentEmail,
                SupervisorEmail: supervisorEmail,
                Sender: input.Author,
                MessageText: input.Text,
                SentAt: input.CreatedAt));

            await auditService.LogActionAsync(
                "SendMessage",
                "Message",
                input.Id,
                new { input.PracticeId, input.Author });

            logger.LogInformation(
                "Message sent for practice {PracticeId}. Sender: {Sender}",
                input.PracticeId,
                input.Author);
        }

        return Results.Created($"/api/messages/{input.PracticeId}", input);
    }
    catch (Exception ex)
    {
        await auditService.LogErrorAsync(
            "SendMessage",
            "Message",
            null,
            ex.Message);

        logger.LogError(ex, "Error sending message for practice {PracticeId}", input.PracticeId);
        return Results.Problem("Failed to send message");
    }
});

app.MapGet("/api/messages/{practiceId:int}", async (
    IMongoDatabase db,
    int practiceId,
    IAuditService auditService) =>
{
    var collection = db.GetCollection<Message>("messages");

    var messages = await collection
        .Find(x => x.PracticeId == practiceId)
        .SortBy(x => x.CreatedAt)
        .ToListAsync();

    await auditService.LogActionAsync(
        "GetMessages",
        "Message",
        null,
        new { PracticeId = practiceId, Count = messages.Count });

    return messages.Count > 0 ? Results.Ok(messages) : Results.NotFound();
});

app.Run();