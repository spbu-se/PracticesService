using CoreService.OutputDirectory;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Current environment
var currentEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Default";

// Register db context
builder.Services.AddDbContext<CoreContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString(currentEnvironment)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/practice", (CoreContext coreContext) =>
{
    var practices = coreContext.Practices;
    return practices;
})
.WithTags("Practice");

app.MapPost("/api/practice", (CoreContext coreContext, Practice practice) =>
{
    coreContext.Attach(practice);
    coreContext.SaveChanges();
})
.WithTags("Practice");

app.Run();

