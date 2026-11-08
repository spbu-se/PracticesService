// <copyright file="LlmClient.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using LlmService.Api.Models;
using Microsoft.Extensions.Options;

/// <summary>
/// Client of any OpenAI-compatible /chat/completions endpoint.
/// </summary>
public class LlmClient : ILlmClient
{
    private const int MaxAttempts = 4;

    private readonly HttpClient http;
    private readonly LlmSettings settings;
    private readonly ILogger<LlmClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LlmClient"/> class.
    /// </summary>
    /// <param name="http">HTTP client.</param>
    /// <param name="settings">LLM settings.</param>
    /// <param name="logger">Logger.</param>
    public LlmClient(HttpClient http, IOptions<LlmSettings> settings, ILogger<LlmClient> logger)
    {
        this.http = http;
        this.settings = settings.Value;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> CompleteAsync(
        string? model,
        double temperature,
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.settings.ApiKey))
        {
            throw new LlmException("LLMSettings:ApiKey is not configured.");
        }

        var url = $"{this.settings.BaseUrl.TrimEnd('/')}/chat/completions";
        var payload = new
        {
            model = model ?? this.settings.Model,
            temperature,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage },
            },
        };

        for (var attempt = 1; ; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(payload) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.settings.ApiKey);

            using var response = await this.http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using var json = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);
                return json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }

            var retryable = response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500;
            if (!retryable || attempt == MaxAttempts)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new LlmException($"LLM provider returned {(int)response.StatusCode}: {body}");
            }

            var delay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5 * attempt);
            if (delay > TimeSpan.FromSeconds(60))
            {
                delay = TimeSpan.FromSeconds(60);
            }

            this.logger.LogWarning("LLM provider returned {Status}, retry {Attempt} in {Delay}s", (int)response.StatusCode, attempt, delay.TotalSeconds);
            await Task.Delay(delay, cancellationToken);
        }
    }
}
