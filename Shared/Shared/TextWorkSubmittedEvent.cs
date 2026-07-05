// <copyright file="TextWorkSubmittedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a text work is submitted.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="Version">The version of the text work.</param>
/// <param name="SubmittedAt">The timestamp when the text work was submitted.</param>
public record TextWorkSubmittedEvent(
    int PracticeId,
    string PracticeTitle,
    string StudentEmail,
    string SupervisorEmail,
    string FileName,
    int Version,
    DateTime SubmittedAt);