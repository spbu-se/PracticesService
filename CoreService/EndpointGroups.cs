// <copyright file="EndpointGroups.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService;

using CoreService.Core;
using CoreService.Core.Models;
using CoreService.Core.Queries;

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
            (CoreContext context) => new ThemesQueries(context).GetThemes().Result);
        group.MapGet(
            "/{themeId:int}",
            (int themeId, CoreContext context) => new ThemesQueries(context).GetThemes(themeId).Result);
        group.MapPost(
            "/",
            (Theme theme, CoreContext context) => new ThemesQueries(context).InsertTheme(theme).Result);
        group.MapPut(
            "/",
            (Theme theme, CoreContext context) =>
                new ThemesQueries(context).UpdateTheme(theme).Result);
        group.MapDelete(
            "/{themeId:int}",
            (int themeId, CoreContext context) => new ThemesQueries(context).DeleteTheme(themeId).Result);

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
        group.MapPost(
            "/",
            (Consultant consultant, CoreContext context) => new ConsultantsQueries(context).InsertConsultant(consultant).Result);
        group.MapPut(
            "/",
            (Consultant consultant, CoreContext context) =>
                new ConsultantsQueries(context).UpdateConsultant(consultant).Result);
        group.MapDelete(
            "/{consultantId:int}",
            (int consultantId, CoreContext context) => new ConsultantsQueries(context).DeleteConsultant(consultantId).Result);

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
            (Group group, CoreContext context) => new GroupsQueries(context).InsertGroup(group).Result);
        group.MapPut(
            "/",
            (Group group, CoreContext context) =>
                new GroupsQueries(context).UpdateGroup(group).Result);
        group.MapDelete(
            "/{groupId:int}",
            (int groupId, CoreContext context) => new GroupsQueries(context).DeleteGroup(groupId).Result);

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
        group.MapPost(
            "/",
            (Lecturer lecturer, CoreContext context) => new LecturersQueries(context).InsertLecturer(lecturer).Result);
        group.MapPut(
            "/",
            (Lecturer lecturer, CoreContext context) =>
                new LecturersQueries(context).UpdateLecturer(lecturer).Result);
        group.MapDelete(
            "/{lecturerId:int}",
            (int lecturerId, CoreContext context) => new LecturersQueries(context).DeleteLecturer(lecturerId).Result);

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
        group.MapPost(
            "/",
            (Practice practice, CoreContext context) => new PracticesQueries(context).InsertPractice(practice).Result);
        group.MapPut(
            "/",
            (Practice practice, CoreContext context) =>
                new PracticesQueries(context).UpdatePractice(practice).Result);
        group.MapDelete(
            "/{practiceId:int}",
            (int practiceId, CoreContext context) => new PracticesQueries(context).DeletePractice(practiceId).Result);

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
        group.MapPost(
            "/",
            (Student student, CoreContext context) => new StudentsQueries(context).InsertStudent(student).Result);
        group.MapPut(
            "/",
            (Student student, CoreContext context) =>
                new StudentsQueries(context).UpdateStudent(student).Result);
        group.MapDelete(
            "/{studentId:int}",
            (int studentId, CoreContext context) => new StudentsQueries(context).DeleteStudent(studentId).Result);

        return group;
    }
}
