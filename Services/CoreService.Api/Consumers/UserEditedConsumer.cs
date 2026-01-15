// <copyright file="UserEditedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Consumers;

using Contracts;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using MassTransit;

/// <summary>
/// Consumer of user editing event.
/// </summary>
public class UserEditedConsumer : IConsumer<UserEditedEvent>
{
    private readonly LecturersQueries lecturersQueries;
    private readonly StudentsQueries studentsQueries;
    private readonly ConsultantsQueries consultantsQueries;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEditedConsumer"/> class.
    /// </summary>
    /// <param name="context">Core DB context.</param>
    public UserEditedConsumer(CoreContext context)
    {
        this.lecturersQueries = new LecturersQueries(context);
        this.studentsQueries = new StudentsQueries(context);
        this.consultantsQueries = new ConsultantsQueries(context);
    }

    /// <summary>
    /// Consumes Event.
    /// </summary>
    /// <param name="context">Consume event context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<UserEditedEvent> context)
    {
        var message = context.Message;

        if (message.RolesToRemove != null && message.RolesToRemove.Any())
        {
            await this.ProcessRemovedRoles(message);
        }

        if (message.RolesToAdd != null && message.RolesToAdd.Any())
        {
            await this.ProcessAddedRoles(message);
        }

        if (message.CurrentRoles != null)
        {
            await this.UpdateNamesForExistingEntities(message);
        }
    }

    private async Task ProcessRemovedRoles(UserEditedEvent message)
    {
        foreach (var role in message.RolesToRemove)
        {
            switch (role)
            {
                case var r when r == RoleNames.GetName(UserRoleType.Supervisor):
                    var lecturer = await this.lecturersQueries.GetLecturerByUserId(message.UserId);
                    if (lecturer != null)
                    {
                        await this.lecturersQueries.DeleteLecturer(lecturer.Id);
                    }

                    break;

                case var r when r == RoleNames.GetName(UserRoleType.Student):
                    var student = await this.studentsQueries.GetStudentByUserId(message.UserId);
                    if (student != null)
                    {
                        await this.studentsQueries.DeleteStudent(student.Id);
                    }

                    break;

                case var r when r == RoleNames.GetName(UserRoleType.Consultant):
                    var consultant = await this.consultantsQueries.GetConsultantByUserId(message.UserId);
                    if (consultant != null)
                    {
                        await this.consultantsQueries.DeleteConsultant(consultant.Id);
                    }

                    break;
            }
        }
    }

    private async Task ProcessAddedRoles(UserEditedEvent message)
    {
        foreach (var role in message.RolesToAdd)
        {
            switch (role)
            {
                case var r when r == RoleNames.GetName(UserRoleType.Supervisor):
                    var lecturer = new Lecturer()
                    {
                        Userid = message.UserId,
                        FirstName = message.FirstName,
                        LastName = message.LastName,
                        MiddleName = message.MiddleName,
                    };
                    await this.lecturersQueries.InsertOrUpdateLecturer(lecturer);
                    break;

                case var r when r == RoleNames.GetName(UserRoleType.Student):
                    var student = new Student()
                    {
                        Userid = message.UserId,
                        FirstName = message.FirstName,
                        LastName = message.LastName,
                        MiddleName = message.MiddleName,
                    };
                    await this.studentsQueries.InsertOrUpdateStudent(student);
                    break;

                case var r when r == RoleNames.GetName(UserRoleType.Consultant):
                    var consultant = new Consultant()
                    {
                        Userid = message.UserId,
                        FirstName = message.FirstName,
                        LastName = message.LastName,
                        MiddleName = message.MiddleName,
                        Contact = message.Username,
                    };
                    await this.consultantsQueries.InsertOrUpdateConsultant(consultant);
                    break;
            }
        }
    }

    private async Task UpdateNamesForExistingEntities(UserEditedEvent message)
    {
        var existingLecturer = await this.lecturersQueries.GetLecturerByUserId(message.UserId);
        if (existingLecturer != null)
        {
            existingLecturer.FirstName = message.FirstName;
            existingLecturer.LastName = message.LastName;
            existingLecturer.MiddleName = message.MiddleName;
            await this.lecturersQueries.UpdateLecturer(existingLecturer);
        }

        var existingStudent = await this.studentsQueries.GetStudentByUserId(message.UserId);
        if (existingStudent != null)
        {
            existingStudent.FirstName = message.FirstName;
            existingStudent.LastName = message.LastName;
            existingStudent.MiddleName = message.MiddleName;
            await this.studentsQueries.UpdateStudent(existingStudent);
        }

        var existingConsultant = await this.consultantsQueries.GetConsultantByUserId(message.UserId);
        if (existingConsultant != null)
        {
            existingConsultant.FirstName = message.FirstName;
            existingConsultant.LastName = message.LastName;
            existingConsultant.MiddleName = message.MiddleName;
            await this.consultantsQueries.UpdateConsultant(existingConsultant);
        }
    }
}