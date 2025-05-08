// <copyright file="ThemesQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Queries;

using CoreService.Api.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Themes table queries.
/// </summary>
/// <param name="context">Core context.</param>
public class ThemesQueries(CoreContext context)
{
    /// <summary>
    /// Gets themes.
    /// </summary>
    /// <param name="id">Theme id, by default set to null.</param>
    /// <returns>List of themes.</returns>
    public async Task<IEnumerable<Theme>> GetThemes(int? id = null)
    {
        var result = context.Themes.AsQueryable();
        if (id != null)
        {
            result = result.Where(theme => theme.Id == id);
        }

        return await result.Include(theme => theme.Consultant).Include(theme => theme.Supervisor).ToListAsync();
    }

    /// <summary>
    /// Inserts new theme.
    /// </summary>
    /// <param name="theme">Input theme.</param>
    /// <returns>Response status.</returns>
    public async Task<int> InsertTheme(Theme theme)
    {
        context.Themes.Add(theme);
        await context.SaveChangesAsync();
        return theme.Id;
    }

    /// <summary>
    /// Updates theme.
    /// </summary>
    /// <param name="theme">Input theme.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> UpdateTheme(Theme theme)
    {
        try
        {
            var prev = await context.Themes.FindAsync(theme.Id);
            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.Updateddate = DateTime.Now;
            prev.Description = string.IsNullOrEmpty(theme.Description) ? prev.Description : theme.Description;
            prev.Title = string.IsNullOrEmpty(theme.Title) ? prev.Title : theme.Title;
            prev.Description = string.IsNullOrEmpty(theme.Description) ? prev.Description : theme.Description;
            prev.Suggestedby = string.IsNullOrEmpty(theme.Suggestedby) ? prev.Suggestedby : theme.Suggestedby;
            prev.Source = string.IsNullOrEmpty(theme.Source) ? prev.Source : theme.Source;
            prev.Department = string.IsNullOrEmpty(theme.Department) ? prev.Department : theme.Department;
            prev.Tags = string.IsNullOrEmpty(theme.Tags) ? prev.Tags : theme.Tags;
            prev.Level = string.IsNullOrEmpty(theme.Level) ? prev.Level : theme.Level;
            prev.Consultantid = theme.Consultantid ?? prev.Consultantid;
            prev.Supervisorid = theme.Supervisorid ?? prev.Supervisorid;
            prev.Isarchived = theme.Isarchived;

            await context.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(exception);
        }
    }

    /// <summary>
    /// Deletes theme.
    /// </summary>
    /// <param name="id">Theme id.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> DeleteTheme(int id)
    {
        var deletedTheme = context.Themes.First(theme => theme.Id == id);
        context.Themes.Remove(deletedTheme);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
