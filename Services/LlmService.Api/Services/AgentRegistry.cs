// <copyright file="AgentRegistry.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using System.Globalization;
using LlmService.Api.Models;
using Microsoft.Extensions.Options;

/// <summary>
/// Loads agents from Agents/*.md. A file consists of an optional front matter
/// (name, title, description, model, temperature, output; one "key: value" per line between --- lines)
/// and the system prompt. Files starting with "_" are ignored.
/// </summary>
public class AgentRegistry : IAgentRegistry
{
    private readonly Dictionary<string, AgentDefinition> agents = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentRegistry"/> class.
    /// </summary>
    /// <param name="settings">LLM settings.</param>
    /// <param name="logger">Logger.</param>
    public AgentRegistry(IOptions<LlmSettings> settings, ILogger<AgentRegistry> logger)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, settings.Value.AgentsPath);
        if (!Directory.Exists(directory))
        {
            logger.LogWarning("Agents directory not found: {Directory}", directory);
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.md").Where(f => !Path.GetFileName(f).StartsWith('_')))
        {
            var agent = Parse(file);
            this.agents[agent.Name] = agent;
            logger.LogInformation("Loaded agent {Agent}", agent.Name);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AgentDefinition> All => this.agents.Values;

    /// <inheritdoc/>
    public AgentDefinition? Find(string name) => this.agents.GetValueOrDefault(name);

    private static AgentDefinition Parse(string file)
    {
        var lines = File.ReadAllText(file).Replace("\r\n", "\n").Split('\n');
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var bodyStart = 0;

        if (lines.Length > 0 && lines[0].Trim() == "---")
        {
            var end = Array.FindIndex(lines, 1, l => l.Trim() == "---");
            if (end > 0)
            {
                foreach (var line in lines[1..end])
                {
                    var separator = line.IndexOf(':');
                    if (separator > 0)
                    {
                        meta[line[..separator].Trim()] = line[(separator + 1)..].Trim().Trim('"');
                    }
                }

                bodyStart = end + 1;
            }
        }

        var name = meta.GetValueOrDefault("name") ?? Path.GetFileNameWithoutExtension(file);
        var temperature = double.TryParse(meta.GetValueOrDefault("temperature"), CultureInfo.InvariantCulture, out var t) ? t : 0.2;

        return new AgentDefinition(
            name,
            meta.GetValueOrDefault("title") ?? name,
            meta.GetValueOrDefault("description") ?? string.Empty,
            string.Join('\n', lines[bodyStart..]).Trim(),
            meta.GetValueOrDefault("model"),
            temperature,
            meta.GetValueOrDefault("output") ?? AgentDefinition.MarkdownOutput);
    }
}
