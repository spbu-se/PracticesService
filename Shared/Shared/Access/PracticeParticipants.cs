// <copyright file="PracticeParticipants.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Access;

/// <summary>
/// Identity ids (user ids from the auth service) of the people who take part in a practice.
/// </summary>
/// <param name="StudentUserId">User id of the student.</param>
/// <param name="SupervisorUserId">User id of the supervisor.</param>
/// <param name="ConsultantUserId">User id of the consultant, if there is one.</param>
public record PracticeParticipants(string? StudentUserId, string? SupervisorUserId, string? ConsultantUserId);
