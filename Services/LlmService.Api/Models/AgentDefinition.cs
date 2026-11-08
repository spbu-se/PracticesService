// <copyright file="AgentDefinition.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Models;

/// <summary>
/// An analysis agent described by an .md file in the Agents directory.
/// </summary>
/// <param name="Name">Agent identifier, equals the file name without extension unless overridden.</param>
/// <param name="Title">Human readable title used in reports.</param>
/// <param name="Description">Short description of the agent.</param>
/// <param name="Prompt">System prompt, the .md body.</param>
/// <param name="Model">Model override, null to use the default one.</param>
/// <param name="Temperature">Sampling temperature.</param>
/// <param name="Output">"markdown" for a free-form answer, "annotations" for a list of findings placed on the PDF.</param>
public record AgentDefinition(
    string Name,
    string Title,
    string Description,
    string Prompt,
    string? Model,
    double Temperature,
    string Output = AgentDefinition.MarkdownOutput)
{
    /// <summary>Free-form Markdown answer.</summary>
    public const string MarkdownOutput = "markdown";

    /// <summary>List of findings that can be highlighted in the PDF.</summary>
    public const string AnnotationsOutput = "annotations";

    /// <summary>Gets a value indicating whether the agent returns findings.</summary>
    public bool ReturnsFindings => string.Equals(this.Output, AnnotationsOutput, StringComparison.OrdinalIgnoreCase);
}
