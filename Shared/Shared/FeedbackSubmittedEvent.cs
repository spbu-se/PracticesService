// <copyright file="FeedbackSubmittedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when feedback is submitted.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="FeedbackType">The type of feedback.</param>
/// <param name="FileName">The name of the attached file.</param>
/// <param name="SubmittedAt">The timestamp when feedback was submitted.</param>
public record FeedbackSubmittedEvent(
    int PracticeId,
    string PracticeTitle,
    string StudentEmail,
    string FeedbackType,
    string FileName,
    DateTime SubmittedAt);