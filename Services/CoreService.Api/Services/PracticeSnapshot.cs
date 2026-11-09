// <copyright file="PracticeSnapshot.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Services;

using Shared.Access;

/// <summary>
/// What the current state of a practice is for the access rules.
/// </summary>
/// <param name="Participants">Participants of the practice.</param>
/// <param name="StudentId">Id of the student record of the practice.</param>
/// <param name="FinalGrade">The current final grade.</param>
public record PracticeSnapshot(PracticeParticipants Participants, int StudentId, string? FinalGrade);
