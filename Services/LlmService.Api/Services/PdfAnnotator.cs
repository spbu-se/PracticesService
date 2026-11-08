// <copyright file="PdfAnnotator.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using LlmService.Api.Models;

/// <summary>
/// Finds quotes in the text layer of a PDF and marks them with highlight annotations.
/// </summary>
public class PdfAnnotator : IPdfAnnotator
{
    private const string Author = "Проверка текста";

    /// <inheritdoc/>
    public byte[] Annotate(byte[] pdf, IReadOnlyCollection<Finding> findings)
    {
        using var reader = new PdfReader(pdf);
        using var output = new MemoryStream();
        var notFound = findings.ToList();

        using (var stamper = new PdfStamper(reader, output))
        {
            for (var page = 1; page <= reader.NumberOfPages; page++)
            {
                var layout = PageLayout.Read(reader, page);
                foreach (var finding in findings)
                {
                    var quote = Normalize(finding.Quote);
                    if (quote.Length == 0)
                    {
                        continue;
                    }

                    var comment = BuildComment(finding);
                    for (var at = layout.Text.IndexOf(quote, StringComparison.Ordinal);
                         at >= 0;
                         at = layout.Text.IndexOf(quote, at + quote.Length, StringComparison.Ordinal))
                    {
                        foreach (var line in layout.LineBoxes(at, quote.Length))
                        {
                            var quad = new[] { line.Left, line.Top, line.Right, line.Top, line.Left, line.Bottom, line.Right, line.Bottom };
                            var annotation = PdfAnnotation.CreateMarkup(
                                stamper.Writer,
                                new Rectangle(line.Left, line.Bottom, line.Right, line.Top),
                                comment,
                                PdfAnnotation.MARKUP_HIGHLIGHT,
                                quad);
                            annotation.Color = BaseColor.YELLOW;
                            annotation.Title = Author;
                            stamper.AddAnnotation(annotation, page);
                        }

                        finding.Page ??= page;
                        notFound.Remove(finding);
                    }
                }
            }

            if (notFound.Count > 0)
            {
                var text = new StringBuilder("Не удалось определить место в документе:\n");
                foreach (var finding in notFound)
                {
                    text.AppendLine(BuildComment(finding));
                }

                var note = PdfAnnotation.CreateText(
                    stamper.Writer,
                    new Rectangle(20, reader.GetPageSize(1).Top - 40, 40, reader.GetPageSize(1).Top - 20),
                    Author,
                    text.ToString(),
                    true,
                    "Comment");
                stamper.AddAnnotation(note, 1);
            }
        }

        return output.ToArray();
    }

    private static string BuildComment(Finding finding)
    {
        var sb = new StringBuilder($"{finding.Type}: «{finding.Quote}»");
        if (!string.IsNullOrWhiteSpace(finding.Suggestion))
        {
            sb.Append($" → «{finding.Suggestion}»");
        }

        if (!string.IsNullOrWhiteSpace(finding.Explanation))
        {
            sb.Append($"\n{finding.Explanation}");
        }

        return sb.ToString();
    }

    // Case, whitespace, yo/ye and dash variants must not prevent a match.
    private static string Normalize(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            var normalized = NormalizeChar(ch);
            if (normalized is not null)
            {
                sb.Append(normalized.Value);
            }
        }

        return sb.ToString();
    }

    private static char? NormalizeChar(char ch)
    {
        if (char.IsWhiteSpace(ch) || ch == '­' || ch == '​')
        {
            return null;
        }

        return ch switch
        {
            'ё' or 'Ё' => 'е',
            '‐' or '‑' or '‒' or '–' or '—' or '−' => '-',
            '«' or '»' or '“' or '”' or '„' => '"',
            '’' or '‘' => '\'',
            _ => char.ToLowerInvariant(ch),
        };
    }

    private readonly record struct Box(float Left, float Bottom, float Right, float Top);

    private sealed class PageLayout
    {
        private readonly List<Box> boxes;

        private PageLayout(string text, List<Box> boxes)
        {
            this.Text = text;
            this.boxes = boxes;
        }

        /// <summary>Gets the normalized page text; index i corresponds to the i-th box.</summary>
        public string Text { get; }

        public static PageLayout Read(PdfReader reader, int page)
        {
            var listener = new CharListener();
            new PdfReaderContentParser(reader).ProcessContent(page, listener);
            return new PageLayout(new string(listener.Chars.ToArray()), listener.Boxes);
        }

        /// <summary>Returns one rectangle per text line covered by the range.</summary>
        public IEnumerable<Box> LineBoxes(int start, int length)
        {
            Box? current = null;
            for (var i = start; i < start + length; i++)
            {
                var box = this.boxes[i];
                if (current is not null && Math.Abs(current.Value.Bottom - box.Bottom) < (box.Top - box.Bottom) / 2)
                {
                    current = new Box(
                        Math.Min(current.Value.Left, box.Left),
                        Math.Min(current.Value.Bottom, box.Bottom),
                        Math.Max(current.Value.Right, box.Right),
                        Math.Max(current.Value.Top, box.Top));
                }
                else
                {
                    if (current is not null)
                    {
                        yield return current.Value;
                    }

                    current = box;
                }
            }

            if (current is not null)
            {
                yield return current.Value;
            }
        }
    }

    private sealed class CharListener : IRenderListener
    {
        public List<char> Chars { get; } = new();

        public List<Box> Boxes { get; } = new();

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
                var descent = character.GetDescentLine();
                var ascent = character.GetAscentLine();
                var box = new Box(
                    Math.Min(descent.GetStartPoint()[0], ascent.GetEndPoint()[0]),
                    Math.Min(descent.GetStartPoint()[1], ascent.GetEndPoint()[1]),
                    Math.Max(descent.GetEndPoint()[0], ascent.GetStartPoint()[0]),
                    Math.Max(descent.GetStartPoint()[1], ascent.GetEndPoint()[1]));

                // One glyph may map to several characters (ligatures), each gets the same box.
                foreach (var ch in character.GetText())
                {
                    var normalized = NormalizeChar(ch);
                    if (normalized is not null)
                    {
                        this.Chars.Add(normalized.Value);
                        this.Boxes.Add(box);
                    }
                }
            }
        }
    }
}
