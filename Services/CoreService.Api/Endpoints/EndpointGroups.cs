// <copyright file="EndpointGroups.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Endpoints;

using Contracts;
using CoreService.Api.Core;
using CoreService.Api.Core.Models;
using CoreService.Api.Core.Queries;
using CoreService.Api.Services;
using MassTransit;
using Shared.Audit;

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
            async (CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger, IAuditService auditService) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                var result = await queries.GetThemes();
                await auditService.LogActionAsync("GetThemes", "Theme", null, new { Count = result.Count() });
                return result;
            });

        group.MapGet(
            "/{themeId:int}",
            async (int themeId, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger, IAuditService auditService) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                var result = await queries.GetThemes(themeId);
                await auditService.LogActionAsync("GetTheme", "Theme", themeId.ToString(), null);
                return result;
            });

        group.MapPost(
            "/",
            async (Theme theme, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger, IAuditService auditService) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                var id = await queries.InsertTheme(theme);
                await auditService.LogActionAsync("CreateTheme", "Theme", id.ToString(), new { Title = theme.Title });
                return id;
            })
            .RequireAuthorization();

        group.MapPut(
            "/",
            async (Theme theme, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger, IAuditService auditService) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                var result = await queries.UpdateTheme(theme);
                await auditService.LogActionAsync("UpdateTheme", "Theme", theme.Id.ToString(), new { Title = theme.Title, IsArchived = theme.Isarchived });
                return result;
            })
            .RequireAuthorization();

        group.MapDelete(
            "/{themeId:int}",
            async (int themeId, CoreContext context, IPublishEndpoint publishEndpoint, UserResolverService userResolver, ILogger<ThemesQueries> logger, IAuditService auditService) =>
            {
                var queries = new ThemesQueries(context, publishEndpoint, userResolver, logger);
                var result = await queries.DeleteTheme(themeId);
                await auditService.LogActionAsync("DeleteTheme", "Theme", themeId.ToString(), null);
                return result;
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
            async (CoreContext context, IAuditService auditService) =>
            {
                var result = await new ConsultantsQueries(context).GetConsultants();
                await auditService.LogActionAsync("GetConsultants", "Consultant", null, new { Count = result.Count() });
                return result;
            });

        group.MapGet(
            "/{consultantId:int}",
            async (int consultantId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new ConsultantsQueries(context).GetConsultants(consultantId);
                await auditService.LogActionAsync("GetConsultant", "Consultant", consultantId.ToString(), null);
                return result;
            });

        group.MapGet(
            "/byUserId",
            async (string userId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new ConsultantsQueries(context).GetConsultantByUserId(userId);
                await auditService.LogActionAsync("GetConsultantByUserId", "Consultant", null, new { UserId = userId });
                return result;
            });

        group.MapPost(
            "/",
            async (Consultant consultant, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("CreateConsultant", "Consultant", consultant.Id.ToString(), new { consultant.Userid });
                return result;
            })
            .RequireAuthorization();

        group.MapPut(
            "/",
            async (Consultant consultant, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("UpdateConsultant", "Consultant", consultant.Id.ToString(), new { consultant.Userid });
                return result;
            })
            .RequireAuthorization();

        group.MapDelete(
            "/{consultantId:int}",
            async (int consultantId, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("DeleteConsultant", "Consultant", consultantId.ToString(), null);
                return result;
            })
            .RequireAuthorization();

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
            async (CoreContext context, IAuditService auditService) =>
            {
                var result = await new GroupsQueries(context).GetGroups();
                await auditService.LogActionAsync("GetGroups", "Group", null, new { Count = result.Count() });
                return result;
            });

        group.MapGet(
            "/{groupId:int}",
            async (int groupId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new GroupsQueries(context).GetGroups(groupId);
                await auditService.LogActionAsync("GetGroup", "Group", groupId.ToString(), null);
                return result;
            });

        group.MapPost(
            "/",
            async (Group group, CoreContext context, IAuditService auditService) =>
            {
                var id = await new GroupsQueries(context).InsertGroup(group);
                await auditService.LogActionAsync("CreateGroup", "Group", id.ToString(), new { group.Name });
                return id;
            })
            .RequireAuthorization();

        group.MapPut(
            "/",
            async (Group group, CoreContext context, IAuditService auditService) =>
            {
                var result = await new GroupsQueries(context).UpdateGroup(group);
                await auditService.LogActionAsync("UpdateGroup", "Group", group.Id.ToString(), new { group.Name });
                return result;
            })
            .RequireAuthorization();

        group.MapDelete(
            "/{groupId:int}",
            async (int groupId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new GroupsQueries(context).DeleteGroup(groupId);
                await auditService.LogActionAsync("DeleteGroup", "Group", groupId.ToString(), null);
                return result;
            })
            .RequireAuthorization();

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
            async (CoreContext context, IAuditService auditService) =>
            {
                var result = await new LecturersQueries(context).GetLecturers();
                await auditService.LogActionAsync("GetLecturers", "Lecturer", null, new { Count = result.Count() });
                return result;
            });

        group.MapGet(
            "/{lecturerId:int}",
            async (int lecturerId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new LecturersQueries(context).GetLecturers(lecturerId);
                await auditService.LogActionAsync("GetLecturer", "Lecturer", lecturerId.ToString(), null);
                return result;
            });

        group.MapGet(
            "/byUserId",
            async (string userId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new LecturersQueries(context).GetLecturerByUserId(userId);
                await auditService.LogActionAsync("GetLecturerByUserId", "Lecturer", null, new { UserId = userId });
                return result;
            });

        group.MapPost(
            "/",
            async (Lecturer lecturer, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("CreateLecturer", "Lecturer", lecturer.Id.ToString(), new { lecturer.Userid });
                return result;
            })
            .RequireAuthorization("AdminOnly");

        group.MapPut(
            "/",
            async (Lecturer lecturer, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("UpdateLecturer", "Lecturer", lecturer.Id.ToString(), new { lecturer.Userid });
                return result;
            })
            .RequireAuthorization("AdminOnly");

        group.MapDelete(
            "/{lecturerId:int}",
            async (int lecturerId, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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

                await auditService.LogActionAsync("DeleteLecturer", "Lecturer", lecturerId.ToString(), null);
                return result;
            })
            .RequireAuthorization("AdminOnly");

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
            async (CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var result = await queries.GetPractices();
                await auditService.LogActionAsync("GetPractices", "Practice", null, new { Count = result.Count() });
                return result;
            });

        group.MapGet(
            "/{practiceId:int}",
            async (int practiceId, CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var practice = await queries.GetPracticeById(practiceId);

                if (practice == null)
                {
                    return Results.NotFound($"Practice with ID {practiceId} not found");
                }

                await auditService.LogActionAsync("GetPractice", "Practice", practiceId.ToString(), null);
                return Results.Ok(practice);
            });

        group.MapGet(
            "/student",
            async (string userId, CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var result = await queries.GetPracticesByStudent(userId);
                await auditService.LogActionAsync("GetPracticesByStudent", "Practice", null, new { UserId = userId });
                return result;
            });

        group.MapGet(
            "/supervisor",
            async (string userId, CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var result = await queries.GetPracticesBySupervisor(userId);
                await auditService.LogActionAsync("GetPracticesBySupervisor", "Practice", null, new { UserId = userId });
                return result;
            });

        group.MapPost(
            "/",
            async (Practice practice, CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var id = await queries.InsertPractice(practice);
                await auditService.LogActionAsync("CreatePractice", "Practice", id.ToString(), new { practice.Type });
                return id;
            })
            .RequireAuthorization();

        group.MapPut(
            "/",
            async (Practice practice, CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var result = await queries.UpdatePractice(practice);
                await auditService.LogActionAsync("UpdatePractice", "Practice", practice.Id.ToString(), new { practice.Status, practice.Finalgrade });
                return result;
            })
            .RequireAuthorization();

        group.MapDelete(
            "/{practiceId:int}",
            async (int practiceId, CoreContext context, IPublishEndpoint publishEndpoint, ILogger<PracticesQueries> logger, IAuditService auditService) =>
            {
                var queries = new PracticesQueries(context, publishEndpoint, logger);
                var result = await queries.DeletePractice(practiceId);
                await auditService.LogActionAsync("DeletePractice", "Practice", practiceId.ToString(), null);
                return result;
            })
            .RequireAuthorization();

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
            async (CoreContext context, IAuditService auditService) =>
            {
                var result = await new StudentsQueries(context).GetStudents();
                await auditService.LogActionAsync("GetStudents", "Student", null, new { Count = result.Count() });
                return result;
            });

        group.MapGet(
            "/{studentId:int}",
            async (int studentId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new StudentsQueries(context).GetStudents(studentId);
                await auditService.LogActionAsync("GetStudent", "Student", studentId.ToString(), null);
                return result;
            });

        group.MapGet(
            "/byUserId",
            async (string userId, CoreContext context, IAuditService auditService) =>
            {
                var result = await new StudentsQueries(context).GetStudentByUserId(userId);
                await auditService.LogActionAsync("GetStudentByUserId", "Student", null, new { UserId = userId });
                return result;
            });

        group.MapPost(
            "/",
            async (Student student, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("CreateStudent", "Student", student.Id.ToString(), new { student.Userid });
                return result;
            })
            .RequireAuthorization();

        group.MapPut(
            "/",
            async (Student student, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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
                await auditService.LogActionAsync("UpdateStudent", "Student", student.Id.ToString(), new { student.Userid });
                return result;
            })
            .RequireAuthorization();

        group.MapDelete(
            "/{studentId:int}",
            async (int studentId, CoreContext context, IPublishEndpoint publishEndpoint, IAuditService auditService) =>
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

                await auditService.LogActionAsync("DeleteStudent", "Student", studentId.ToString(), null);
                return result;
            })
            .RequireAuthorization();

        return group;
    }
}
