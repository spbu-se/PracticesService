using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PracticeEntities.Models;

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

builder.Services.AddSwaggerGen();

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
    IConfiguration config) =>
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

    return Results.Ok(feedback);
}).DisableAntiforgery();

app.MapGet("/api/feedbacks/{practiceId:int}", async (
    int practiceId,
    [FromQuery] string? type,
    IMongoDatabase db) =>
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

    return Results.Ok(feedbacks);
});

app.MapPost("/api/text-works", async (
    [FromForm] TextWork model,
    IAmazonS3 s3Client,
    IMongoDatabase db,
    IConfiguration config) =>
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

    return Results.Ok(textWork);
}).DisableAntiforgery();

app.MapGet("/api/text-works/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db) =>
{
    var collection = db.GetCollection<TextWork>("textworks");

    var filter = Builders<TextWork>.Filter.Eq(t => t.PracticeId, practiceId);
    var textWorks = await collection.Find(filter)
        .SortByDescending(t => t.Version)
        .ToListAsync();

    return Results.Ok(textWorks);
});

app.MapGet("/api/text-works/latest/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db) =>
{
    var collection = db.GetCollection<TextWork>("textworks");

    var filter = Builders<TextWork>.Filter.Eq(t => t.PracticeId, practiceId);
    var latest = await collection.Find(filter)
        .SortByDescending(t => t.Version)
        .FirstOrDefaultAsync();

    return latest is not null ? Results.Ok(latest) : Results.NotFound();
});

app.MapPost("/api/text-works/{id}/comments", async (
    IMongoDatabase db,
    string id,
    Comment input) =>
{
    var collection = db.GetCollection<TextWork>("textworks");
    var filter = Builders<TextWork>.Filter.Eq(t => t.Id, id);
    var update = Builders<TextWork>.Update.Push(t => t.Comments, input);

    var result = await collection.UpdateOneAsync(filter, update);

    return result.MatchedCount > 0 ? Results.Ok() : Results.NotFound();
});

app.MapPost("/api/presentations", async (
    [FromForm] Presentation model,
    IAmazonS3 s3Client,
    IMongoDatabase db,
    IConfiguration config) =>
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

    return Results.Ok(presentation);
}).DisableAntiforgery();

app.MapGet("/api/presentations/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db) =>
{
    var collection = db.GetCollection<Presentation>("presentations");

    var filter = Builders<Presentation>.Filter.Eq(p => p.PracticeId, practiceId);
    var presentations = await collection.Find(filter)
        .SortByDescending(p => p.Version)
        .ToListAsync();

    return Results.Ok(presentations);
});

app.MapGet("/api/presentations/latest/{practiceId:int}", async (
    int practiceId,
    IMongoDatabase db) =>
{
    var collection = db.GetCollection<Presentation>("presentations");

    var filter = Builders<Presentation>.Filter.Eq(t => t.PracticeId, practiceId);
    var latest = await collection.Find(filter)
        .SortByDescending(t => t.Version)
        .FirstOrDefaultAsync();

    return latest is not null ? Results.Ok(latest) : Results.NotFound();
});

app.MapPost("/api/presentations/{id}/comments", async (
    IMongoDatabase db,
    string id,
    Comment input) =>
{
    var collection = db.GetCollection<Presentation>("presentations");
    var filter = Builders<Presentation>.Filter.Eq(t => t.Id, id);
    var update = Builders<Presentation>.Update.Push(t => t.Comments, input);

    var result = await collection.UpdateOneAsync(filter, update);

    return result.MatchedCount > 0 ? Results.Ok() : Results.NotFound();
});

app.MapPost("/api/goals-tasks", async (IMongoDatabase db, GoalsAndTasks input) =>
{
    var collection = db.GetCollection<GoalsAndTasks>("goals_tasks");

    var existing = await collection.Find(x => x.PracticeId == input.PracticeId).FirstOrDefaultAsync();

    if (existing != null)
    {
        existing.Goals = input.Goals;
        existing.Tasks = input.Tasks;
        existing.CreatedAt = DateTime.UtcNow;
        await collection.ReplaceOneAsync(x => x.Id == existing.Id, existing);
        return Results.Ok(existing);
    }

    input.Id = null;
    await collection.InsertOneAsync(input);
    return Results.Created($"/goals-tasks/{input.PracticeId}", input);
});

app.MapGet("/api/goals-tasks/{practiceId:int}", async (IMongoDatabase db, int practiceId) =>
{
    var collection = db.GetCollection<GoalsAndTasks>("goals_tasks");

    var result = await collection.Find(x => x.PracticeId == practiceId).FirstOrDefaultAsync();

    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/api/reports", async (IMongoDatabase db, Report input) =>
{
    var collection = db.GetCollection<Report>("reports");
    input.Id = null;
    input.CreatedAt = DateTime.UtcNow;
    await collection.InsertOneAsync(input);
    return Results.Created($"/api/reports/{input.PracticeId}", input);
});

app.MapGet("/api/reports/{practiceId:int}", async (IMongoDatabase db, int practiceId) =>
{
    var collection = db.GetCollection<Report>("reports");
    var reports = await collection
        .Find(r => r.PracticeId == practiceId)
        .SortByDescending(r => r.CreatedAt)
        .ToListAsync();

    return Results.Ok(reports);
});

app.MapPost("/api/reports/{id}/comments", async (IMongoDatabase db, string id, Comment input) =>
{
    var collection = db.GetCollection<Report>("reports");
    var filter = Builders<Report>.Filter.Eq(r => r.Id, id);
    var update = Builders<Report>.Update.Push(r => r.Comments, input);

    var result = await collection.UpdateOneAsync(filter, update);

    return result.MatchedCount > 0 ? Results.Ok() : Results.NotFound();
});

app.MapPost("/api/repositories", async (IMongoDatabase db, Repository input) =>
{
    var collection = db.GetCollection<Repository>("repositories");

    var existing = await collection.Find(r => r.PracticeId == input.PracticeId).FirstOrDefaultAsync();

    if (existing != null)
    {
        existing.RepositoryLink = input.RepositoryLink;
        existing.AccountName = input.AccountName;
        existing.UploadedAt = DateTime.UtcNow;

        await collection.ReplaceOneAsync(r => r.Id == existing.Id, existing);
        return Results.Ok(existing);
    }

    input.Id = null;
    input.UploadedAt = DateTime.UtcNow;

    await collection.InsertOneAsync(input);
    return Results.Created($"/api/repositories/{input.PracticeId}", input);
});

app.MapGet("/api/repositories/{practiceId:int}", async (IMongoDatabase db, int practiceId) =>
{
    var collection = db.GetCollection<Repository>("repositories");
    var repo = await collection.Find(r => r.PracticeId == practiceId).FirstOrDefaultAsync();

    return repo != null ? Results.Ok(repo) : Results.NotFound();
});

app.MapPost("/api/messages", async (IMongoDatabase db, Message input) =>
{
    var collection = db.GetCollection<Message>("messages");

    if (input.CreatedAt == default)
    {
        input.CreatedAt = DateTime.UtcNow;
    }

    await collection.InsertOneAsync(input);

    return Results.Created($"/api/messages/{input.PracticeId}", input);
});

app.MapGet("/api/messages/{practiceId:int}", async (IMongoDatabase db, int practiceId) =>
{
    var collection = db.GetCollection<Message>("messages");

    var messages = await collection
        .Find(x => x.PracticeId == practiceId)
        .SortBy(x => x.CreatedAt)
        .ToListAsync();

    return messages.Count > 0 ? Results.Ok(messages) : Results.NotFound();
});

app.Run();
