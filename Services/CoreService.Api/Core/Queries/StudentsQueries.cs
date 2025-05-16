// <copyright file="StudentsQueries.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Core.Queries;

using CoreService.Api.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Students table queries.
/// </summary>
/// <param name="context">Core context.</param>
public class StudentsQueries(CoreContext context)
{
    /// <summary>
    /// Gets Students.
    /// </summary>
    /// <param name="id">Student id, by default set to null.</param>
    /// <returns>List of students.</returns>
    public async Task<IEnumerable<Student>> GetStudents(int? id = null)
    {
        var result = context.Students.AsQueryable();
        if (id != null)
        {
            result = result.Where(student => student.Id == id);
        }

        return await result.Include(s => s.Group).Include(s => s.Practices).ToListAsync();
    }

    /// <summary>
    /// Gets Student by UserId.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <returns>Student with selected user id.</returns>
    public async Task<Student?> GetStudentByUserId(string userId)
    {
        var result = await context.Students.FirstOrDefaultAsync(s => s.Userid == userId);

        return result;
    }

    /// <summary>
    /// Inserts new student.
    /// </summary>
    /// <param name="student">Input student.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> InsertOrUpdateStudent(Student student)
    {
        var prev = await context.Students.FindAsync(student.Id);
        if (prev != null)
        {
            return await this.UpdateStudent(student);
        }

        context.Students.Add(student);
        await context.SaveChangesAsync();
        return Results.Ok(student.Id);
    }

    /// <summary>
    /// Updates student.
    /// </summary>
    /// <param name="student">Input student.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> UpdateStudent(Student student)
    {
        try
        {
            var prev = await context.Students.FindAsync(student.Id);
            if (prev == null)
            {
                return Results.BadRequest();
            }

            prev.FirstName = string.IsNullOrEmpty(student.FirstName) ? prev.FirstName : student.FirstName;
            prev.LastName = string.IsNullOrEmpty(student.LastName) ? prev.LastName : student.LastName;
            prev.MiddleName = string.IsNullOrEmpty(student.MiddleName) ? prev.MiddleName : student.MiddleName;
            prev.Groupid = student.Groupid;
            await context.SaveChangesAsync();
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(exception);
        }
    }

    /// <summary>
    /// Deletes student.
    /// </summary>
    /// <param name="id">Student id.</param>
    /// <returns>Response status.</returns>
    public async Task<IResult> DeleteStudent(int id)
    {
        var deletedStudent = context.Students.First(student => student.Id == id);
        context.Students.Remove(deletedStudent);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
