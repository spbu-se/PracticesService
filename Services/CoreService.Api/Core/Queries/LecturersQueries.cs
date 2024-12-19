// <copyright file="LecturersQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Queries;

using CoreService.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Lecturers table queries.
/// </summary>
/// <param name="context">Core context.</param>
public class LecturersQueries(CoreContext context)
{
    /// <summary>
    /// Gets Lecturers.
    /// </summary>
    /// <param name="id">Lecturer id, by default set to null.</param>
    /// <returns>List of lecturers.</returns>
    public async Task<IEnumerable<Lecturer>> GetLecturers(int? id = null)
    {
        var result = context.Lecturers.AsQueryable();
        if (id != null)
        {
            result = result.Where(lecturer => lecturer.Id == id);
        }

        return await result.ToListAsync();
    }

    /// <summary>
    /// Inserts new lecturer.
    /// </summary>
    /// <param name="lecturer">Input lecturer.</param>
    /// <returns>Response status.</returns>
    public async Task<int> InsertLecturer(Lecturer lecturer)
    {
        context.Lecturers.Add(lecturer);
        await context.SaveChangesAsync();
        return lecturer.Id;
    }

    /// <summary>
    /// Updates lecturer.
    /// </summary>
    /// <param name="lecturer">Input lecturer.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> UpdateLecturer(Lecturer lecturer)
    {
        try
        {
            var prev = await context.Lecturers.FindAsync(lecturer.Id);
            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.Department = lecturer.Department;
            prev.Cansupervisevkr = lecturer.Cansupervisevkr;
            await context.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(exception);
        }
    }

    /// <summary>
    /// Deletes lecturer.
    /// </summary>
    /// <param name="id">Lecturer id.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> DeleteLecturer(int id)
    {
        var deletedLecturer = context.Lecturers.First(lecturer => lecturer.Id == id);
        context.Lecturers.Remove(deletedLecturer);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
