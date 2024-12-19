// <copyright file="GroupsQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core.Queries;

using CoreService.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Groups table queries.
/// </summary>
/// <param name="context">Core context.</param>
public class GroupsQueries(CoreContext context)
{
    /// <summary>
    /// Gets Groups.
    /// </summary>
    /// <param name="id">Group id, by default set to null.</param>
    /// <returns>List of groups.</returns>
    public async Task<IEnumerable<Group>> GetGroups(int? id = null)
    {
        var result = context.Groups.AsQueryable();
        if (id != null)
        {
            result = result.Where(group => group.Id == id);
        }

        return await result.ToListAsync();
    }

    /// <summary>
    /// Inserts new group.
    /// </summary>
    /// <param name="group">Input group.</param>
    /// <returns>Response status.</returns>
    public async Task<int> InsertGroup(Group group)
    {
        context.Groups.Add(group);
        await context.SaveChangesAsync();
        return group.Id;
    }

    /// <summary>
    /// Updates group.
    /// </summary>
    /// <param name="group">Input group.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> UpdateGroup(Group group)
    {
        try
        {
            var prev = await context.Groups.FindAsync(group.Id);
            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.Name = group.Name;
            prev.Program = group.Program;
            prev.Year = group.Year;
            await context.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(exception);
        }
    }

    /// <summary>
    /// Deletes group.
    /// </summary>
    /// <param name="id">Group id.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> DeleteGroup(int id)
    {
        var deletedGroup = context.Groups.First(group => group.Id == id);
        context.Groups.Remove(deletedGroup);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
