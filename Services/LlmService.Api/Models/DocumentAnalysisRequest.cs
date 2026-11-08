// <copyright file="DocumentAnalysisRequest.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Models;

/// <summary>
/// Request model for document analysis.
/// </summary>
public class DocumentAnalysisRequest
{
    /// <summary>
    /// Gets or sets the PDF file to analyze.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of document: report, presentation or feedback.
    /// </summary>
    public string DocumentType { get; set; } = "report";

    /// <summary>
    /// Gets or sets comma separated agent names. Empty means the default agents.
    /// </summary>
    public string? Agents { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return a PDF report instead of JSON.
    /// </summary>
    public bool GeneratePdf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return the original PDF with the findings highlighted as comments.
    /// </summary>
    public bool Annotate { get; set; }
}
