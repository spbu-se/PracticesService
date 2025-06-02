using MongoDB.Driver;
using PracticeEntities.Models;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();
