// <copyright file="PresentationCommentAddedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a comment is added to a presentation.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="PresentationId">The ID of the presentation.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="Version">The version of the presentation.</param>
/// <param name="Author">The author of the comment.</param>
/// <param name="CommentText">The text content of the comment.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="CreatedAt">The timestamp when the comment was created.</param>
public record PresentationCommentAddedEvent(
    int PracticeId,
    string PracticeTitle,
    string PresentationId,
    string FileName,
    int Version,
    string Author,
    string CommentText,
    string? StudentEmail,
    string? SupervisorEmail,
    DateTime CreatedAt);