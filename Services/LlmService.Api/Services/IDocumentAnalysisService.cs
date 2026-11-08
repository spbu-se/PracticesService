// <copyright file="IDocumentAnalysisService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using LlmService.Api.Models;

/// <summary>
/// Runs agents over document text.
/// </summary>
public interface IDocumentAnalysisService
{
    /// <summary>
    /// Runs the given agents over the text.
    /// </summary>
    /// <param name="text">Document text.</param>
    /// <param name="documentType">Type of document (report, presentation, feedback).</param>
    /// <param name="agents">Agents to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An answer per agent.</returns>
    Task<List<AgentResult>> AnalyzeAsync(string text, string documentType, IEnumerable<AgentDefinition> agents, CancellationToken cancellationToken);
}
