// <copyright file="PracticesQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Queries;

using CoreService.Api.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Practices table queries.
/// </summary>
/// <param name="context">Core context.</param>
public class PracticesQueries(CoreContext context)
{
    /// <summary>
    /// Gets Practices.
    /// </summary>
    /// <param name="id">Practice id, by default set to null.</param>
    /// <returns>List of practices.</returns>
    public async Task<IEnumerable<Practice>> GetPractices(int? id = null)
    {
        var result = context.Practices.AsQueryable();
        if (id != null)
        {
            result = result.Where(practice => practice.Id == id);
        }

        return await result.ToListAsync();
    }

    /// <summary>
    /// Gets queried Practices.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <returns>List of practices.</returns>
    public async Task<IEnumerable<Practice>> GetQueriedPractices(string userId)
    {
        var result = context.Practices.Include(p => p.Student).AsQueryable();
        if (string.IsNullOrEmpty(userId))
        {
            return new List<Practice>();
        }

        result = result.Where(p => p.Student.Userid == userId);

        return await result.ToListAsync();
    }

    /// <summary>
    /// Inserts new practice.
    /// </summary>
    /// <param name="practice">Input practice.</param>
    /// <returns>Response status.</returns>
    public async Task<int> InsertPractice(Practice practice)
    {
        context.Practices.Add(practice);
        await context.SaveChangesAsync();
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
            var prev = await context.Practices.FindAsync(practice.Id);
            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.Themeid = practice.Themeid;
            prev.Consultantid = practice.Consultantid;
            prev.Supervisorid = practice.Supervisorid;
            prev.Studentid = practice.Studentid;
            prev.Finalgrade = practice.Finalgrade;
            prev.Status = string.IsNullOrEmpty(practice.Status) ? prev.Status : practice.Status;
            prev.Updateddate = DateTime.UtcNow;
            prev.Type = string.IsNullOrEmpty(practice.Type) ? prev.Type : practice.Type;
            await context.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
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
        var deletedPractice = context.Practices.First(practice => practice.Id == id);
        context.Practices.Remove(deletedPractice);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
