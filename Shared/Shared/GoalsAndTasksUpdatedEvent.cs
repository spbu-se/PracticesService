// <copyright file="GoalsAndTasksUpdatedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when practice goals and tasks are updated.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="UpdatedAt">The timestamp when the update occurred.</param>
public record GoalsAndTasksUpdatedEvent(
    int PracticeId,
    string PracticeTitle,
    string StudentEmail,
    string? SupervisorEmail,
    DateTime UpdatedAt);