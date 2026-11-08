// <copyright file="IPdfGenerator.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using LlmService.Api.Models;

/// <summary>
/// Interface for PDF generation.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Generates a PDF report from document analysis results.
    /// </summary>
    /// <param name="result">Document analysis result.</param>
    /// <returns>PDF file bytes.</returns>
    byte[] GenerateDocumentReport(DocumentAnalysisResult result);
}
