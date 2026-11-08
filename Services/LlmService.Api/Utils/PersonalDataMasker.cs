// <copyright file="PersonalDataMasker.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Utils;

using System.Text.RegularExpressions;

/// <summary>
/// Masks obvious personal data before the text leaves the service.
/// </summary>
public static partial class PersonalDataMasker
{
    /// <summary>
    /// Replaces e-mails, links, phone numbers and Russian full names with placeholders.
    /// </summary>
    /// <param name="text">Source text.</param>
    /// <returns>Masked text.</returns>
    public static string Mask(string text)
    {
        text = EmailRegex().Replace(text, "[EMAIL]");
        text = UrlRegex().Replace(text, "[ССЫЛКА]");
        text = PhoneRegex().Replace(text, "[ТЕЛЕФОН]");
        text = FullNameRegex().Replace(text, "[ФИО]");
        text = ShortNameRegex().Replace(text, "[ФИО]");
        return text;
    }

    [GeneratedRegex(@"[\w.+-]+@[\w-]+(\.[\w-]+)+")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(https?://|www\.)\S+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"(?<!\w)(\+7|8)[\s(-]*\d{3}[\s)-]*\d{3}[\s-]*\d{2}[\s-]*\d{2}(?!\d)")]
    private static partial Regex PhoneRegex();

    // Фамилия Имя Отчество
    [GeneratedRegex(@"\b[А-ЯЁ][а-яё]+\s+[А-ЯЁ][а-яё]+\s+[А-ЯЁ][а-яё]+(вич|вна|ична|ьич)\b")]
    private static partial Regex FullNameRegex();

    // Фамилия И.О. / И.О. Фамилия
    [GeneratedRegex(@"\b[А-ЯЁ][а-яё]+\s+[А-ЯЁ]\.\s?[А-ЯЁ]\.|\b[А-ЯЁ]\.\s?[А-ЯЁ]\.\s?[А-ЯЁ][а-яё]+")]
    private static partial Regex ShortNameRegex();
}
