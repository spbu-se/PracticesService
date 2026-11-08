// <copyright file="IAgentRegistry.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using LlmService.Api.Models;

/// <summary>
/// Source of agents defined by .md files.
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// Gets all loaded agents.
    /// </summary>
    IReadOnlyCollection<AgentDefinition> All { get; }

    /// <summary>
    /// Finds an agent by name.
    /// </summary>
    /// <param name="name">Agent name, case insensitive.</param>
    /// <returns>The agent or null.</returns>
    AgentDefinition? Find(string name);
}
