// <copyright file="ILlmClient.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

/// <summary>
/// Client of a chat completion API.
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Sends a system + user message pair and returns the model answer.
    /// </summary>
    /// <param name="model">Model override, null for the default one.</param>
    /// <param name="temperature">Sampling temperature.</param>
    /// <param name="systemPrompt">System prompt.</param>
    /// <param name="userMessage">User message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Model answer.</returns>
    Task<string> CompleteAsync(string? model, double temperature, string systemPrompt, string userMessage, CancellationToken cancellationToken);
}

/// <summary>
/// Raised when the LLM provider is unavailable or rejects the request.
/// </summary>
public class LlmException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LlmException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    public LlmException(string message)
        : base(message)
    {
    }
}
