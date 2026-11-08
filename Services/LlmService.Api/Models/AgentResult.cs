// <copyright file="AgentResult.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Models;

/// <summary>
/// Result of a single agent.
/// </summary>
/// <param name="Agent">Agent name.</param>
/// <param name="Title">Agent title.</param>
/// <param name="Content">Agent answer in Markdown.</param>
/// <param name="Findings">Findings, only for agents that return them.</param>
public record AgentResult(string Agent, string Title, string Content, List<Finding>? Findings = null);
