// <copyright file="Program.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using LlmService.Api.Models;
using LlmService.Api.Services;
using LlmService.Api.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var gatewayBasePath = builder.Configuration["Swagger:GatewayBasePath"] ?? "/api";

builder.Services.AddSwaggerGen(c =>
{
    c.AddServer(new OpenApiServer
    {
        Url = gatewayBasePath,
        Description = "Gateway endpoint",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Only the listed origins (comma separated, Cors:AllowedOrigins) may call the API from a browser with credentials.
var corsOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins(corsOrigins)
              .AllowAnyHeader());
});

builder.Services.AddAuthentication("GatewayAuth")
    .AddScheme<AuthenticationSchemeOptions, GatewayAuthHandler.GatewayAuthHandler>("GatewayAuth", null);
builder.Services.AddAuthorization();

builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection("LLMSettings"));
builder.Services.AddSingleton<IAgentRegistry, AgentRegistry>();
builder.Services.AddHttpClient<ILlmClient, LlmClient>(client => client.Timeout = TimeSpan.FromMinutes(3));
builder.Services.AddScoped<IDocumentAnalysisService, DocumentAnalysisService>();
builder.Services.AddSingleton<IPdfGenerator, MarkdownPdfGenerator>();
builder.Services.AddSingleton<IPdfAnnotator, PdfAnnotator>();

var app = builder.Build();

app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/analysis/agents", (IAgentRegistry registry) =>
        registry.All.Select(a => new { a.Name, a.Title, a.Description }))
    .WithName("GetAgents")
    .RequireAuthorization();

app.MapPost("/api/analysis/analyze", async (
        [FromForm] DocumentAnalysisRequest request,
        IAgentRegistry registry,
        IDocumentAnalysisService analysisService,
        IPdfGenerator pdfGenerator,
        IPdfAnnotator pdfAnnotator,
        IOptions<LlmSettings> settings,
        ILogger<Program> logger,
        CancellationToken cancellationToken) =>
    {
        if (request.File is null || request.File.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        if (!string.Equals(Path.GetExtension(request.File.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { error = "Only PDF files are supported" });
        }

        var documentType = request.DocumentType.ToLowerInvariant();
        if (documentType is not ("report" or "presentation" or "feedback"))
        {
            return Results.BadRequest(new { error = "Document type must be: report, presentation, or feedback" });
        }

        var names = string.IsNullOrWhiteSpace(request.Agents)
            ? settings.Value.DefaultAgents
            : request.Agents.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var agents = new List<AgentDefinition>();
        foreach (var name in names)
        {
            var agent = registry.Find(name);
            if (agent is null)
            {
                return Results.BadRequest(new
                {
                    error = $"Unknown agent '{name}'",
                    available = registry.All.Select(a => a.Name),
                });
            }

            agents.Add(agent);
        }

        byte[] pdf;
        string text;
        try
        {
            // The file is kept in memory only and never written to disk.
            using var memory = new MemoryStream();
            await request.File.CopyToAsync(memory, cancellationToken);
            pdf = memory.ToArray();
            text = PdfTextService.ExtractText(new MemoryStream(pdf));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot read PDF {FileName}", request.File.FileName);
            return Results.BadRequest(new { error = "Cannot read the PDF file" });
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return Results.BadRequest(new { error = "No text found in the PDF (scanned documents are not supported)" });
        }

        logger.LogInformation(
            "Analyzing {Length} characters as {DocumentType} with agents {Agents}",
            text.Length,
            documentType,
            string.Join(",", names));

        try
        {
            var result = new DocumentAnalysisResult
            {
                FileName = request.File.FileName,
                DocumentType = documentType,
                AnalyzedAt = DateTime.UtcNow,
                Results = await analysisService.AnalyzeAsync(text, documentType, agents, cancellationToken),
            };

            if (request.Annotate)
            {
                var findings = result.Results.Where(r => r.Findings is not null).SelectMany(r => r.Findings!).ToList();
                if (findings.Count == 0 && result.Results.All(r => r.Findings is null))
                {
                    return Results.BadRequest(new { error = "None of the selected agents returns findings, annotate is not applicable" });
                }

                return Results.File(
                    pdfAnnotator.Annotate(pdf, findings),
                    "application/pdf",
                    $"Annotated_{Path.GetFileNameWithoutExtension(request.File.FileName)}.pdf");
            }

            if (request.GeneratePdf)
            {
                return Results.File(
                    pdfGenerator.GenerateDocumentReport(result),
                    "application/pdf",
                    $"Analysis_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }

            return Results.Ok(result);
        }
        catch (LlmException ex)
        {
            logger.LogError(ex, "LLM provider error");
            return Results.Problem(title: "LLM provider error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("AnalyzeDocument")
    .RequireAuthorization()
    .Produces<DocumentAnalysisResult>()
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status502BadGateway)
    .DisableAntiforgery();

app.Run();
