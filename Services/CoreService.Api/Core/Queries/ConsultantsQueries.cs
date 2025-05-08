// <copyright file="ConsultantsQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Queries;

using CoreService.Api.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Consultants table queries.
/// </summary>
/// <param name="context">Core context.</param>
public class ConsultantsQueries(CoreContext context)
{
    /// <summary>
    /// Gets Consultants.
    /// </summary>
    /// <param name="id">Consultant id, by default set to null.</param>
    /// <returns>List of consultants.</returns>
    public async Task<IEnumerable<Consultant>> GetConsultants(int? id = null)
    {
        var result = context.Consultants.AsQueryable();
        if (id != null)
        {
            result = result.Where(consultant => consultant.Id == id);
        }

        return await result.ToListAsync();
    }

    /// <summary>
    /// Inserts new consultant.
    /// </summary>
    /// <param name="consultant">Input consultant.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> InsertOrUpdateConsultant(Consultant consultant)
    {
        var prev = await context.Consultants.FindAsync(consultant.Id);
        if (prev == null)
        {
            return await this.UpdateConsultant(consultant);
        }

        context.Consultants.Add(consultant);
        await context.SaveChangesAsync();
        return Results.Ok(consultant.Id);
    }

    /// <summary>
    /// Updates consultant.
    /// </summary>
    /// <param name="consultant">Input consultant.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> UpdateConsultant(Consultant consultant)
    {
        try
        {
            var prev = await context.Consultants.FindAsync(consultant.Id);
            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.FirstName = string.IsNullOrEmpty(consultant.FirstName) ? prev.FirstName : consultant.FirstName;
            prev.LastName = string.IsNullOrEmpty(consultant.LastName) ? prev.LastName : consultant.LastName;
            prev.MiddleName = string.IsNullOrEmpty(consultant.MiddleName) ? prev.MiddleName : consultant.MiddleName;
            prev.Contact = string.IsNullOrEmpty(consultant.Contact) ? prev.Contact : consultant.Contact;
            await context.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(exception);
        }
    }

    /// <summary>
    /// Deletes consultant.
    /// </summary>
    /// <param name="id">Consultant id.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> DeleteConsultant(int id)
    {
        var deletedConsultant = context.Consultants.First(consultant => consultant.Id == id);
        context.Consultants.Remove(deletedConsultant);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
