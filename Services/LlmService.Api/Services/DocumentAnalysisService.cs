// <copyright file="DocumentAnalysisService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using System.Text;
using LlmService.Api.Models;
using LlmService.Api.Utils;
using Microsoft.Extensions.Options;

/// <summary>
/// Splits the text into chunks and runs every agent over them.
/// </summary>
public class DocumentAnalysisService : IDocumentAnalysisService
{
    private readonly ILlmClient llm;
    private readonly LlmSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentAnalysisService"/> class.
    /// </summary>
    /// <param name="llm">LLM client.</param>
    /// <param name="settings">LLM settings.</param>
    public DocumentAnalysisService(ILlmClient llm, IOptions<LlmSettings> settings)
    {
        this.llm = llm;
        this.settings = settings.Value;
    }

    /// <inheritdoc/>
    public async Task<List<AgentResult>> AnalyzeAsync(
        string text,
        string documentType,
        IEnumerable<AgentDefinition> agents,
        CancellationToken cancellationToken)
    {
        if (this.settings.MaskPersonalData)
        {
            text = PersonalDataMasker.Mask(text);
        }

        var chunks = Split(text, this.settings.MaxChunkChars);
        var results = new List<AgentResult>();

        // Sequential on purpose: free tiers have tight rate limits.
        foreach (var agent in agents)
        {
            var content = new StringBuilder();
            var findings = new List<Finding>();
            var summaries = new List<string>();
            for (var i = 0; i < chunks.Count; i++)
            {
                var message = BuildMessage(documentType, chunks[i], i + 1, chunks.Count, this.settings.MaskPersonalData);
                var answer = await this.llm.CompleteAsync(agent.Model, agent.Temperature, agent.Prompt, message, cancellationToken);

                if (agent.ReturnsFindings)
                {
                    if (FindingsParser.TryParse(answer, out var summary, out var parsed))
                    {
                        summaries.Add(summary);
                        findings.AddRange(parsed);
                    }
                    else
                    {
                        content.AppendLine(answer.Trim()).AppendLine();
                    }

                    continue;
                }

                if (chunks.Count > 1)
                {
                    content.AppendLine($"#### Фрагмент {i + 1} из {chunks.Count}").AppendLine();
                }

                content.AppendLine(answer.Trim()).AppendLine();
            }

            if (!agent.ReturnsFindings)
            {
                results.Add(new AgentResult(agent.Name, agent.Title, content.ToString().Trim()));
                continue;
            }

            findings = findings
                .Where(f => !string.IsNullOrWhiteSpace(f.Quote))
                .DistinctBy(f => (f.Quote.Trim(), f.Suggestion?.Trim()))
                .ToList();
            results.Add(new AgentResult(agent.Name, agent.Title, FindingsParser.ToMarkdown(summaries, findings) + content, findings));
        }

        return results;
    }

    private static string BuildMessage(string documentType, string chunk, int index, int total, bool masked)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Тип документа: {documentType}. Фрагмент {index} из {total}.");
        if (masked)
        {
            sb.AppendLine("Персональные данные заменены метками в квадратных скобках ([EMAIL], [ФИО] и т.п.) — это не ошибки.");
        }

        sb.AppendLine().AppendLine("Текст:").AppendLine(chunk);
        return sb.ToString();
    }

    private static List<string> Split(string text, int maxChars)
    {
        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var line in text.Split('\n'))
        {
            var rest = line;
            do
            {
                var part = rest.Length > maxChars ? rest[..maxChars] : rest;
                rest = rest[part.Length..];

                if (current.Length > 0 && current.Length + part.Length + 1 > maxChars)
                {
                    chunks.Add(current.ToString());
                    current.Clear();
                }

                current.AppendLine(part);
            }
            while (rest.Length > 0);
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString());
        }

        return chunks;
    }
}
