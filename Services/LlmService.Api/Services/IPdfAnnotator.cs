// <copyright file="IPdfAnnotator.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace LlmService.Api.Services;

using LlmService.Api.Models;

/// <summary>
/// Highlights findings in the original PDF.
/// </summary>
public interface IPdfAnnotator
{
    /// <summary>
    /// Adds a highlight with a comment for every occurrence of every finding quote.
    /// Findings that cannot be found are listed in one note on the first page.
    /// Sets <see cref="Finding.Page"/> of the findings that were found.
    /// </summary>
    /// <param name="pdf">Original PDF.</param>
    /// <param name="findings">Findings to place.</param>
    /// <returns>Annotated PDF.</returns>
    byte[] Annotate(byte[] pdf, IReadOnlyCollection<Finding> findings);
}
