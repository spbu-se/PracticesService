// <copyright file="TextWorkCommentAddedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a comment is added to a text work.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="TextWorkId">The ID of the text work.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="Version">The version of the text work.</param>
/// <param name="Author">The author of the comment.</param>
/// <param name="CommentText">The text content of the comment.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="CreatedAt">The timestamp when the comment was created.</param>
public record TextWorkCommentAddedEvent(
    int PracticeId,
    string PracticeTitle,
    string TextWorkId,
    string FileName,
    int Version,
    string Author,
    string CommentText,
    string? StudentEmail,
    string? SupervisorEmail,
    DateTime CreatedAt);