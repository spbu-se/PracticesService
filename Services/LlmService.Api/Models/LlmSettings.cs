// <copyright file="LlmSettings.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Models;

/// <summary>
/// Settings of the OpenAI-compatible LLM provider.
/// </summary>
public class LlmSettings
{
    /// <summary>
    /// Gets or sets the base URL of an OpenAI-compatible API (Groq, OpenRouter, Ollama, vLLM, ...).
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";

    /// <summary>
    /// Gets or sets the API key. Never store it in appsettings.json, use the LLMSettings__ApiKey environment variable.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default model, an agent may override it in its .md file.
    /// </summary>
    public string Model { get; set; } = "openai/gpt-oss-120b";

    /// <summary>
    /// Gets or sets the seed sent with every request, so that repeated checks of the same text give the same answer.
    /// Null disables it. Providers apply it on a best-effort basis.
    /// </summary>
    public int? Seed { get; set; } = 42;

    /// <summary>
    /// Gets or sets the reasoning effort (low, medium, high) for reasoning models. Null omits the parameter,
    /// which is required for providers and models that do not support it.
    /// </summary>
    public string? ReasoningEffort { get; set; } = "medium";

    /// <summary>
    /// Gets or sets the limit of generated tokens per request. Reasoning models spend part of it on thinking,
    /// so a small limit leaves the visible answer empty.
    /// </summary>
    public int MaxOutputTokens { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the directory with agent .md files, relative to the application directory.
    /// </summary>
    public string AgentsPath { get; set; } = "Agents";

    /// <summary>
    /// Gets or sets the maximum number of characters sent to the model in one request.
    /// </summary>
    public int MaxChunkChars { get; set; } = 3000;

    /// <summary>
    /// Gets or sets a value indicating whether personal data (e-mails, phones, links, full names) is masked before sending.
    /// </summary>
    public bool MaskPersonalData { get; set; } = true;

    /// <summary>
    /// Gets or sets the agents that run when the request does not name any.
    /// </summary>
    public List<string> DefaultAgents { get; set; } = new() { "spell-checker" };
}
