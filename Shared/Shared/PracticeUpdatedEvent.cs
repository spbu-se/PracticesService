// <copyright file="PracticeUpdatedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a practice is updated.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="SupervisorEmail">The email of the supervisor (optional).</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="UpdatedAt">The timestamp when the update occurred.</param>
/// <param name="PracticeTitle">The title of the practice (optional).</param>
public record PracticeUpdatedEvent(
    int PracticeId,
    string? SupervisorEmail,
    string StudentEmail,
    DateTime UpdatedAt,
    string? PracticeTitle);