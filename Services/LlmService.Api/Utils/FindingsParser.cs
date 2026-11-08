// <copyright file="FindingsParser.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Utils;

using System.Text;
using System.Text.Json;
using LlmService.Api.Models;

/// <summary>
/// Parses the JSON answer of agents with "annotations" output and renders it as Markdown.
/// </summary>
public static class FindingsParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Parses an answer of the form {"summary": "...", "findings": [...]}, tolerating text or code fences around the JSON.
    /// </summary>
    /// <param name="answer">Raw model answer.</param>
    /// <param name="summary">Parsed summary.</param>
    /// <param name="findings">Parsed findings.</param>
    /// <returns>True if the answer contained valid JSON.</returns>
    public static bool TryParse(string answer, out string summary, out List<Finding> findings)
    {
        summary = string.Empty;
        findings = new List<Finding>();

        var start = answer.IndexOf('{');
        var end = answer.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return false;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Payload>(answer[start..(end + 1)], Options);
            if (parsed is null)
            {
                return false;
            }

            summary = parsed.Summary ?? string.Empty;
            findings = parsed.Findings ?? new List<Finding>();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Renders findings as Markdown, used in JSON responses and the summary PDF report.
    /// </summary>
    /// <param name="summaries">Summaries of all chunks.</param>
    /// <param name="findings">Findings.</param>
    /// <returns>Markdown text.</returns>
    public static string ToMarkdown(IEnumerable<string> summaries, IReadOnlyCollection<Finding> findings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("### Сводка");
        foreach (var summary in summaries.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            sb.AppendLine(summary.Trim());
        }

        sb.AppendLine($"Найдено замечаний: {findings.Count}.").AppendLine();
        sb.AppendLine("### Найденные ошибки");
        foreach (var f in findings)
        {
            sb.AppendLine($"- **{f.Type}**: «{f.Quote}» → «{f.Suggestion}»");
            if (!string.IsNullOrWhiteSpace(f.Explanation))
            {
                sb.AppendLine($"  {f.Explanation}");
            }
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private sealed class Payload
    {
        public string? Summary { get; set; }

        public List<Finding>? Findings { get; set; }
    }
}
