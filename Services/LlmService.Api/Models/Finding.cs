// <copyright file="Finding.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Models;

/// <summary>
/// A single remark of an agent about a place in the text.
/// </summary>
public class Finding
{
    /// <summary>Gets or sets the kind of the remark, e.g. orthography.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the exact fragment of the document the remark is about.</summary>
    public string Quote { get; set; } = string.Empty;

    /// <summary>Gets or sets the proposed replacement.</summary>
    public string? Suggestion { get; set; }

    /// <summary>Gets or sets the explanation.</summary>
    public string? Explanation { get; set; }

    /// <summary>Gets or sets the first page the quote was found on, null if it was not found.</summary>
    public int? Page { get; set; }
}
