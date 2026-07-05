// <copyright file="ReportCommentAddedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a comment is added to a report.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="ReportId">The ID of the report.</param>
/// <param name="Author">The author of the comment.</param>
/// <param name="CommentText">The text content of the comment.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="CreatedAt">The timestamp when the comment was created.</param>
public record ReportCommentAddedEvent(
    int PracticeId,
    string PracticeTitle,
    string ReportId,
    string Author,
    string CommentText,
    string? StudentEmail,
    string? SupervisorEmail,
    DateTime CreatedAt);