// <copyright file="MarkdownPdfGenerator.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using LlmService.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

/// <summary>
/// Renders agent answers (Markdown headings and lists) into a PDF report.
/// </summary>
public class MarkdownPdfGenerator : IPdfGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownPdfGenerator"/> class.
    /// </summary>
    public MarkdownPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc/>
    public byte[] GenerateDocumentReport(DocumentAnalysisResult result)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"Анализ документа: {result.FileName}")
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Item().Text($"Тип документа: {result.DocumentType}").Bold();
                    column.Item().Text($"Дата анализа: {result.AnalyzedAt.ToLocalTime():dd.MM.yyyy HH:mm}");
                    column.Item().PaddingBottom(5).LineHorizontal(1);

                    foreach (var agentResult in result.Results)
                    {
                        column.Item().PaddingTop(12).Text(agentResult.Title).FontSize(16).Bold();

                        foreach (var line in agentResult.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            if (line.StartsWith('#'))
                            {
                                column.Item().PaddingTop(8).Text(line.TrimStart('#').Trim()).FontSize(13).Bold();
                            }
                            else
                            {
                                column.Item().Text(line.Replace("**", string.Empty));
                            }
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
