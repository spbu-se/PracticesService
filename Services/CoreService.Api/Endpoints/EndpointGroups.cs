// <copyright file="EndpointGroups.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

using CoreService.Api.Services;

namespace CoreService.Api.Endpoints;

using Contracts;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using MassTransit;

/// <summary>
/// Endpoints groups.
/// </summary>
public static class EndpointGroups
{
    /// <summary>
    /// Themes endpoints.
    /// </summary>
    /// <param name="group"><inheritdoc cref="RouteGroupBuilder" /></param>
    /// <returns>Themes route group builder.</returns>
    public static RouteGroupBuilder ThemesGroup(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async (CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                return await queries.GetThemes();
            });

        group.MapGet(
            "/{themeId:int}",
            async (int themeId, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                return await queries.GetThemes(themeId);
            });

        group.MapPost(
            "/",
            async (Theme theme, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                return await queries.InsertTheme(theme);
            })
            .RequireAuthorization();

        group.MapPut(
            "/",
            async (Theme theme, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                return await queries.UpdateTheme(theme);
            })
            .RequireAuthorization();

        group.MapDelete(
            "/{themeId:int}",
            async (int themeId, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                return await queries.DeleteTheme(themeId);
            })
            .RequireAuthorization();

        return group;
    }

    /// <summary>
    /// Consultants endpoints.
    /// </summary>
    /// <param name="group"><inheritdoc cref="RouteGroupBuilder" /></param>
    /// <returns>Consultants route group builder.</returns>
    public static RouteGroupBuilder ConsultantsGroup(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            (CoreContext context) => new ConsultantsQueries(context).GetConsultants().Result);
        group.MapGet(
            "/{consultantId:int}",
            (int consultantId, CoreContext context) => new ConsultantsQueries(context).GetConsultants(consultantId).Result);
        group.MapGet(
            "/byUserId",
            (string userId, CoreContext context) => new ConsultantsQueries(context).GetConsultantByUserId(userId).Result);
        group.MapPost(
            "/",
            async (Consultant consultant, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var result = await new ConsultantsQueries(context).InsertOrUpdateConsultant(consultant);
                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        consultant.Userid,
                        consultant.FirstName,
                        consultant.LastName,
                        consultant.MiddleName,
                        UserActionType.Create,
                        RoleNames.GetName(UserRoleType.Consultant),
                        DateTime.UtcNow));
                return result;
            }).RequireAuthorization();
        group.MapPut(
            "/",
            async (Consultant consultant, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var result = await new ConsultantsQueries(context).UpdateConsultant(consultant);
                var prev = await context.Consultants.FindAsync(consultant.Id);
                if (prev == null)
                {
                    return Results.BadRequest();
                }

                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        consultant.Userid,
                        string.IsNullOrEmpty(consultant.FirstName) ? prev.FirstName : consultant.FirstName,
                        string.IsNullOrEmpty(consultant.LastName) ? prev.LastName : consultant.LastName,
                        string.IsNullOrEmpty(consultant.MiddleName) ? prev.MiddleName : consultant.MiddleName,
                        UserActionType.Update,
                        RoleNames.GetName(UserRoleType.Consultant),
                        DateTime.UtcNow));
                return result;
            }).RequireAuthorization();
        group.MapDelete(
            "/{consultantId:int}",
            async (int consultantId, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var consultant = await context.Consultants.FindAsync(consultantId);
                var result = await new ConsultantsQueries(context).DeleteConsultant(consultantId);
                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        consultant.Userid,
                        consultant.FirstName,
                        consultant.LastName,
                        consultant.MiddleName,
                        UserActionType.Delete,
                        RoleNames.GetName(UserRoleType.Consultant),
                        DateTime.UtcNow));
                return result;
            }).RequireAuthorization();

        return group;
    }

    /// <summary>
    /// Groups endpoints.
    /// </summary>
    /// <param name="group"><inheritdoc cref="RouteGroupBuilder" /></param>
    /// <returns>Groups route group builder.</returns>
    public static RouteGroupBuilder GroupsGroup(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            (CoreContext context) => new GroupsQueries(context).GetGroups().Result);
        group.MapGet(
            "/{groupId:int}",
            (int groupId, CoreContext context) => new GroupsQueries(context).GetGroups(groupId).Result);
        group.MapPost(
            "/",
            (Group group, CoreContext context) => new GroupsQueries(context).InsertGroup(group).Result).RequireAuthorization();
        group.MapPut(
            "/",
            (Group group, CoreContext context) =>
                new GroupsQueries(context).UpdateGroup(group).Result).RequireAuthorization();
        group.MapDelete(
            "/{groupId:int}",
            (int groupId, CoreContext context) => new GroupsQueries(context).DeleteGroup(groupId).Result).RequireAuthorization();

        return group;
    }

    /// <summary>
    /// Lecturers endpoints.
    /// </summary>
    /// <param name="group"><inheritdoc cref="RouteGroupBuilder" /></param>
    /// <returns>Lecturers route group builder.</returns>
    public static RouteGroupBuilder LecturersGroup(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            (CoreContext context) => new LecturersQueries(context).GetLecturers().Result);
        group.MapGet(
            "/{lecturerId:int}",
            (int lecturerId, CoreContext context) => new LecturersQueries(context).GetLecturers(lecturerId).Result);
        group.MapGet(
            "/byUserId",
            (string userId, CoreContext context) => new LecturersQueries(context).GetLecturerByUserId(userId).Result);
        group.MapPost(
            "/",
            async (Lecturer lecturer, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var result = await new LecturersQueries(context).InsertOrUpdateLecturer(lecturer);
                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        lecturer.Userid,
                        lecturer.FirstName,
                        lecturer.LastName,
                        lecturer.MiddleName,
                        UserActionType.Create,
                        RoleNames.GetName(UserRoleType.Supervisor),
                        DateTime.UtcNow));

                return result;
            }).RequireAuthorization("AdminOnly");
        group.MapPut(
            "/",
            async (Lecturer lecturer, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var result = await new LecturersQueries(context).UpdateLecturer(lecturer);
                var prev = await context.Lecturers.FindAsync(lecturer.Id);
                if (prev == null)
                {
                    return Results.BadRequest();
                }

                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        lecturer.Userid,
                        string.IsNullOrEmpty(lecturer.FirstName) ? prev.FirstName : lecturer.FirstName,
                        string.IsNullOrEmpty(lecturer.LastName) ? prev.LastName : lecturer.LastName,
                        string.IsNullOrEmpty(lecturer.MiddleName) ? prev.MiddleName : lecturer.MiddleName,
                        UserActionType.Update,
                        RoleNames.GetName(UserRoleType.Supervisor),
                        DateTime.UtcNow));
                return result;
            }).RequireAuthorization("AdminOnly");
        group.MapDelete(
            "/{lecturerId:int}",
            async (int lecturerId, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var lecturer = await context.Lecturers.FindAsync(lecturerId);
                var result = await new LecturersQueries(context).DeleteLecturer(lecturerId);

                if (lecturer != null)
                {
                    await publishEndpoint.Publish(
                        new UserWithRoleActionEvent(
                            lecturer.Userid,
                            lecturer.FirstName,
                            lecturer.LastName,
                            lecturer.MiddleName,
                            UserActionType.Delete,
                            RoleNames.GetName(UserRoleType.Supervisor),
                            DateTime.UtcNow));
                }

                return result;
            }).RequireAuthorization("AdminOnly");

        return group;
    }

    /// <summary>
    /// Practices endpoints.
    /// </summary>
    /// <param name="group"><inheritdoc cref="RouteGroupBuilder" /></param>
    /// <returns>Practices route group builder.</returns>
    public static RouteGroupBuilder PracticesGroup(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            (CoreContext context) => new PracticesQueries(context).GetPractices().Result);
        group.MapGet(
            "/{practiceId:int}",
            (int practiceId, CoreContext context) => new PracticesQueries(context).GetPractices(practiceId).Result);
        group.MapGet(
            "/student",
            (string userId, CoreContext context) => new PracticesQueries(context).GetPracticesByStudent(userId).Result);
        group.MapGet(
            "/supervisor",
            (string userId, CoreContext context) => new PracticesQueries(context).GetPracticesBySupervisor(userId).Result);
        group.MapPost(
            "/",
            (Practice practice, CoreContext context) => new PracticesQueries(context).InsertPractice(practice).Result).RequireAuthorization();
        group.MapPut(
            "/",
            (Practice practice, CoreContext context) =>
                new PracticesQueries(context).UpdatePractice(practice).Result).RequireAuthorization();
        group.MapDelete(
            "/{practiceId:int}",
            (int practiceId, CoreContext context) => new PracticesQueries(context).DeletePractice(practiceId).Result).RequireAuthorization();

        return group;
    }

    /// <summary>
    /// Students endpoints.
    /// </summary>
    /// <param name="group"><inheritdoc cref="RouteGroupBuilder" /></param>
    /// <returns>Students route group builder.</returns>
    public static RouteGroupBuilder StudentsGroup(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            (CoreContext context) => new StudentsQueries(context).GetStudents().Result);
        group.MapGet(
            "/{studentId:int}",
            (int studentId, CoreContext context) => new StudentsQueries(context).GetStudents(studentId).Result);
        group.MapGet(
            "/byUserId",
            (string userId, CoreContext context) => new StudentsQueries(context).GetStudentByUserId(userId).Result);
        group.MapPost(
            "/",
            async (Student student, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var result = await new StudentsQueries(context).InsertOrUpdateStudent(student);
                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        student.Userid,
                        student.FirstName,
                        student.LastName,
                        student.MiddleName,
                        UserActionType.Create,
                        RoleNames.GetName(UserRoleType.Student),
                        DateTime.UtcNow));
                return result;
            }).RequireAuthorization();
        group.MapPut(
            "/",
            async (Student student, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var prev = await context.Students.FindAsync(student.Id);
                if (prev == null)
                {
                    return Results.BadRequest();
                }

                var result = await new StudentsQueries(context).UpdateStudent(student);

                await publishEndpoint.Publish(
                    new UserWithRoleActionEvent(
                        student.Userid,
                        string.IsNullOrEmpty(student.FirstName) ? prev.FirstName : student.FirstName,
                        string.IsNullOrEmpty(student.LastName) ? prev.LastName : student.LastName,
                        string.IsNullOrEmpty(student.MiddleName) ? prev.MiddleName : student.MiddleName,
                        UserActionType.Update,
                        RoleNames.GetName(UserRoleType.Student),
                        DateTime.UtcNow));
                return result;
            }).RequireAuthorization();
        group.MapDelete(
            "/{studentId:int}",
            async (int studentId, CoreContext context, IPublishEndpoint publishEndpoint) =>
            {
                var student = await context.Students.FindAsync(studentId);
                var result = await new StudentsQueries(context).DeleteStudent(studentId);

                if (student != null)
                {
                    await publishEndpoint.Publish(
                        new UserWithRoleActionEvent(
                            student.Userid,
                            student.FirstName,
                            student.LastName,
                            student.MiddleName,
                            UserActionType.Delete,
                            RoleNames.GetName(UserRoleType.Student),
                            DateTime.UtcNow));
                }

                return result;
            }).RequireAuthorization();

        return group;
    }
}
