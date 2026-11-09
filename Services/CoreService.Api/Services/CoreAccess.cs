// <copyright file="CoreAccess.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Services;

using CoreService.Api.Core;
using Microsoft.EntityFrameworkCore;
using Shared.Access;

/// <summary>
/// Reads the facts the access rules need: who takes part in a practice and who owns a record.
/// </summary>
public class CoreAccess
{
    private readonly CoreContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreAccess"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    public CoreAccess(CoreContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Gets the participants and the grade of a practice.
    /// </summary>
    /// <param name="practiceId">Practice id.</param>
    /// <returns>The snapshot or null if the practice does not exist.</returns>
    public Task<PracticeSnapshot?> GetPracticeAsync(int practiceId)
        => this.context.Practices
            .AsNoTracking()
            .Where(p => p.Id == practiceId)
            .Select(p => new PracticeSnapshot(
                new PracticeParticipants(
                    p.Student.Userid,
                    p.Supervisor.Userid,
                    p.Consultant != null ? p.Consultant.Userid : null),
                p.Studentid,
                p.Finalgrade))
            .FirstOrDefaultAsync();

    /// <summary>
    /// Gets the user id of the student record.
    /// </summary>
    /// <param name="studentId">Student id.</param>
    /// <returns>The user id or null.</returns>
    public Task<string?> GetStudentUserIdAsync(int studentId)
        => this.context.Students.AsNoTracking().Where(s => s.Id == studentId).Select(s => (string?)s.Userid).FirstOrDefaultAsync();

    /// <summary>
    /// Gets the user id and the supervision flag of the lecturer record.
    /// </summary>
    /// <param name="lecturerId">Lecturer id.</param>
    /// <returns>The facts or null if the lecturer does not exist.</returns>
    public async Task<(string UserId, bool CanSuperviseVkr)?> GetLecturerAsync(int lecturerId)
    {
        var lecturer = await this.context.Lecturers.AsNoTracking().Where(l => l.Id == lecturerId)
            .Select(l => new { l.Userid, l.Cansupervisevkr }).FirstOrDefaultAsync();
        return lecturer is null ? null : (lecturer.Userid, lecturer.Cansupervisevkr);
    }

    /// <summary>
    /// Gets the user id of the consultant record.
    /// </summary>
    /// <param name="consultantId">Consultant id.</param>
    /// <returns>The user id, null if the consultant does not exist or is not linked to a user.</returns>
    public Task<string?> GetConsultantUserIdAsync(int consultantId)
        => this.context.Consultants.AsNoTracking().Where(c => c.Id == consultantId).Select(c => c.Userid).FirstOrDefaultAsync();

    /// <summary>
    /// Gets the user who suggested the theme.
    /// </summary>
    /// <param name="themeId">Theme id.</param>
    /// <returns>The user id of the author, an empty string if there is none, null if the theme does not exist.</returns>
    public async Task<string?> GetThemeAuthorAsync(int themeId)
    {
        var theme = await this.context.Themes.AsNoTracking().Where(t => t.Id == themeId).Select(t => new { t.Suggestedby }).FirstOrDefaultAsync();
        return theme is null ? null : theme.Suggestedby ?? string.Empty;
    }
}
