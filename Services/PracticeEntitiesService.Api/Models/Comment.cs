// <copyright file="Comment.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Models;

/// <summary>
/// Represents a comment associated with a report.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the author of the comment.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text content of the comment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}