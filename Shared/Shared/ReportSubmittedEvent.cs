// <copyright file="ReportSubmittedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a report is submitted.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="SubmittedAt">The timestamp when the report was submitted.</param>
public record ReportSubmittedEvent(
    int PracticeId,
    string PracticeTitle,
    string? StudentEmail,
    string? SupervisorEmail,
    DateTime SubmittedAt);