// <copyright file="PracticesQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Queries;

using Contracts;
using CoreService.Api.Core.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Practices table queries.
/// </summary>
public class PracticesQueries
{
    private readonly CoreContext context;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly ILogger<PracticesQueries> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PracticesQueries"/> class.
    /// </summary>
    /// <param name="context">Core context.</param>
    /// <param name="publishEndpoint">MassTransit publish endpoint.</param>
    /// <param name="logger">Logger instance.</param>
    public PracticesQueries(
        CoreContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<PracticesQueries> logger)
    {
        this.context = context;
        this.publishEndpoint = publishEndpoint;
        this.logger = logger;
    }

    /// <summary>
    /// Gets Practices.
    /// </summary>
    /// <param name="id">Practice id, by default set to null.</param>
    /// <returns>List of practices.</returns>
    public async Task<IEnumerable<Practice>> GetPractices(int? id = null)
    {
        var result = this.context.Practices.Include(p => p.Supervisor).Include(p => p.Consultant).Include(p => p.Theme).Include(p => p.Student).ThenInclude(s => s.Group).AsQueryable();
        if (id != null)
        {
            result = result.Where(practice => practice.Id == id);
        }

        return await result.ToListAsync();
    }

    /// <summary>
    /// Gets queried Practices.
    /// </summary>
    /// <param name="userId">Student user id.</param>
    /// <returns>List of practices.</returns>
    public async Task<IEnumerable<Practice>> GetPracticesByStudent(string userId)
    {
        var result = this.context.Practices.Include(p => p.Theme).Include(p => p.Student).Include(p => p.Supervisor).Include(p => p.Consultant).AsQueryable();
        if (string.IsNullOrEmpty(userId))
        {
            return new List<Practice>();
        }

        result = result.Where(p => p.Student.Userid == userId);

        return await result.ToListAsync();
    }

    /// <summary>
    /// Gets practices supervised by a given supervisor user id.
    /// </summary>
    /// <param name="supervisorUserId">Supervisor user id.</param>
    /// <returns>List of practices supervised by this supervisor.</returns>
    public async Task<IEnumerable<Practice>> GetPracticesBySupervisor(string supervisorUserId)
    {
        var result = this.context.Practices
            .Include(p => p.Theme)
            .Include(p => p.Student)
            .Include(p => p.Supervisor)
            .Include(p => p.Consultant)
            .AsQueryable();

        if (string.IsNullOrEmpty(supervisorUserId))
        {
            return new List<Practice>();
        }

        result = result.Where(p => p.Supervisor != null && p.Supervisor.Userid == supervisorUserId);

        return await result.ToListAsync();
    }

    /// <summary>
    /// Gets a single practice by ID.
    /// </summary>
    /// <param name="id">Practice id.</param>
    /// <returns>Practice or null if not found.</returns>
    public async Task<Practice?> GetPracticeById(int id)
    {
        return await this.context.Practices
            .Include(p => p.Supervisor)
            .Include(p => p.Consultant)
            .Include(p => p.Theme)
            .Include(p => p.Student)
            .ThenInclude(s => s.Group)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Inserts new practice.
    /// </summary>
    /// <param name="practice">Input practice.</param>
    /// <returns>Response status.</returns>
    public async Task<int> InsertPractice(Practice practice)
    {
        this.context.Practices.Add(practice);
        await this.context.SaveChangesAsync();
        return practice.Id;
    }

    /// <summary>
    /// Updates practice.
    /// </summary>
    /// <param name="practice">Input practice.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> UpdatePractice(Practice practice)
    {
        try
        {
            var prev = await this.context.Practices
                .Include(p => p.Supervisor)
                .Include(p => p.Student)
                .Include(p => p.Theme)
                .FirstOrDefaultAsync(p => p.Id == practice.Id);

            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.Themeid = practice.Themeid;
            prev.Consultantid = practice.Consultantid;
            prev.Supervisorid = practice.Supervisorid;
            prev.Studentid = practice.Studentid;
            prev.Finalgrade = practice.Finalgrade;
            prev.Updateddate = DateTime.Now;
            prev.Type = string.IsNullOrEmpty(practice.Type) ? prev.Type : practice.Type;

            // Update status based on Finalgrade
            if (!string.IsNullOrEmpty(practice.Finalgrade))
            {
                prev.Status = "Завершено";
            }
            else
            {
                prev.Status = "Не завершено";
            }

            await this.context.SaveChangesAsync();

            // Publish practice updated event
            var studentEmail = prev.Student?.Email ?? prev.Student?.Userid ?? "unknown";
            var supervisorEmail = prev.Supervisor?.Email;
            var practiceTitle = prev.Theme?.Title ?? "Practice";

            await this.publishEndpoint.Publish(new PracticeUpdatedEvent(
                PracticeId: prev.Id,
                SupervisorEmail: supervisorEmail,
                StudentEmail: studentEmail,
                UpdatedAt: DateTime.UtcNow,
                PracticeTitle: practiceTitle));

            this.logger.LogInformation(
                "Practice {PracticeId} updated. Student: {StudentEmail}, Status: {Status}",
                prev.Id,
                studentEmail,
                prev.Status);

            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            this.logger.LogError(exception, "Error updating practice {PracticeId}", practice.Id);
            return Results.BadRequest(exception);
        }
    }

    /// <summary>
    /// Deletes practice.
    /// </summary>
    /// <param name="id">Practice id.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> DeletePractice(int id)
    {
        var deletedPractice = this.context.Practices.First(practice => practice.Id == id);
        this.context.Practices.Remove(deletedPractice);
        await this.context.SaveChangesAsync();
        return Results.Ok();
    }
}
