// <copyright file="PracticeAccess.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Access;

using System.Security.Claims;
using Contracts;

/// <summary>
/// Who may do what with a practice and the data that belongs to it. The same rules are used by every service.
/// </summary>
public static class PracticeAccess
{
    /// <summary>
    /// Finds how the caller is related to the practice.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>The relation, <see cref="PracticeRelation.None"/> for strangers.</returns>
    public static PracticeRelation RelationOf(ClaimsPrincipal user, PracticeParticipants practice)
    {
        var id = user.UserId();
        if (string.IsNullOrEmpty(id))
        {
            return PracticeRelation.None;
        }

        if (id == practice.StudentUserId)
        {
            return PracticeRelation.Student;
        }

        if (id == practice.SupervisorUserId)
        {
            return PracticeRelation.Supervisor;
        }

        return id == practice.ConsultantUserId ? PracticeRelation.Consultant : PracticeRelation.None;
    }

    /// <summary>
    /// Reading a practice and everything attached to it, reviewers may read too.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>True if allowed.</returns>
    public static bool CanRead(ClaimsPrincipal user, PracticeParticipants practice)
        => user.IsInternalService() || user.IsManager() || user.HasRole(UserRoleType.Reviewer) || RelationOf(user, practice) != PracticeRelation.None;

    /// <summary>
    /// Listing every practice of the system.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <returns>True for managers and reviewers.</returns>
    public static bool CanListAll(ClaimsPrincipal user) => user.IsManager() || user.HasRole(UserRoleType.Reviewer);

    /// <summary>
    /// Adding reports, works, goals, messages and comments.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>True if allowed.</returns>
    public static bool CanContribute(ClaimsPrincipal user, PracticeParticipants practice) => CanRead(user, practice);

    /// <summary>
    /// Changing the theme, supervisor and consultant of a practice.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>True if allowed.</returns>
    public static bool CanEdit(ClaimsPrincipal user, PracticeParticipants practice)
        => user.IsManager() || RelationOf(user, practice) is PracticeRelation.Student or PracticeRelation.Supervisor;

    /// <summary>
    /// Deleting a practice (refusing the theme).
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>True if allowed.</returns>
    public static bool CanDelete(ClaimsPrincipal user, PracticeParticipants practice) => CanEdit(user, practice);

    /// <summary>
    /// Setting the final grade, a student must not grade the own practice.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>True if allowed.</returns>
    public static bool CanGrade(ClaimsPrincipal user, PracticeParticipants practice)
        => user.IsManager() || RelationOf(user, practice) == PracticeRelation.Supervisor;

    /// <summary>
    /// Uploading a feedback (review) of the practice, it is written by the supervisor or the consultant.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="practice">Participants of the practice.</param>
    /// <returns>True if allowed.</returns>
    public static bool CanLeaveFeedback(ClaimsPrincipal user, PracticeParticipants practice)
        => user.IsManager() || RelationOf(user, practice) is PracticeRelation.Supervisor or PracticeRelation.Consultant;
}
