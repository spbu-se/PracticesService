// <copyright file="PdfTextService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Utils;

using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

/// <summary>
/// PDF text extractor.
/// </summary>
public static class PdfTextService
{
    // A gap wider than this part of the line height is a word space. A normal space is about 0.28 em,
    // kerning and tracking stay well below, so the threshold sits between them. The default iText strategy
    // is much more eager and splits words in justified text ("сис темы").
    private const float WordGapRatio = 0.2f;

    /// <summary>
    /// Extracts text from PDF file.
    /// </summary>
    /// <param name="pdf">PDF content.</param>
    /// <returns>Extracted text.</returns>
    public static string ExtractText(Stream pdf)
    {
        var text = new StringBuilder();

        using var reader = new PdfReader(pdf);
        var parser = new PdfReaderContentParser(reader);

        for (var page = 1; page <= reader.NumberOfPages; page++)
        {
            var listener = new LineListener();
            parser.ProcessContent(page, listener);
            listener.Finish();

            var pageText = JoinHyphenated(listener.Lines);
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                text.AppendLine(pageText);
                text.AppendLine();
            }
        }

        return text.ToString().Trim();
    }

    // "си-" at the end of a line + "стемы" at the start of the next one is one word. The hyphen is kept
    // because it may be a real one ("веб-" + "сервис"); the agents are told that such hyphens are not errors.
    private static string JoinHyphenated(List<string> lines)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var joinsNext = i + 1 < lines.Count
                && line.Length > 1
                && (line[^1] == '-' || line[^1] == '­')
                && char.IsLetter(line[^2])
                && char.IsLower(lines[i + 1][0]);

            if (joinsNext)
            {
                sb.Append(line[^1] == '­' ? line[..^1] : line);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString().Trim();
    }

    private sealed class LineListener : IRenderListener
    {
        private readonly StringBuilder line = new();
        private float previousEnd;
        private float previousBaseline;
        private bool hasPrevious;
        private bool pendingSpace;

        public List<string> Lines { get; } = new();

        public void BeginTextBlock()
        {
        }

        public void EndTextBlock()
        {
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
            foreach (var character in renderInfo.GetCharacterRenderInfos())
            {
                var value = character.GetText().Replace(' ', ' ');
                var baseline = character.GetBaseline();
                var start = baseline.GetStartPoint()[0];
                var end = baseline.GetEndPoint()[0];
                var y = baseline.GetStartPoint()[1];
                var height = Math.Abs(character.GetAscentLine().GetStartPoint()[1] - character.GetDescentLine().GetStartPoint()[1]);
                if (height < 1)
                {
                    height = 10;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    this.pendingSpace = this.line.Length > 0;
                    this.previousEnd = end;
                    continue;
                }

                if (this.hasPrevious)
                {
                    if (Math.Abs(y - this.previousBaseline) > height * 0.5f || start < this.previousEnd - height)
                    {
                        this.FlushLine();
                    }
                    else if (this.pendingSpace || start - this.previousEnd > height * WordGapRatio)
                    {
                        this.line.Append(' ');
                    }
                }

                this.line.Append(value);
                this.previousEnd = end;
                this.previousBaseline = y;
                this.hasPrevious = true;
                this.pendingSpace = false;
            }
        }

        public void Finish() => this.FlushLine();

        private void FlushLine()
        {
            var value = this.line.ToString().Trim();
            if (value.Length > 0)
            {
                this.Lines.Add(value);
            }

            this.line.Clear();
            this.hasPrevious = false;
            this.pendingSpace = false;
        }
    }
}
