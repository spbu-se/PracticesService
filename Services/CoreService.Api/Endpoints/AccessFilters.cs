// <copyright file="AccessFilters.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Endpoints;

using Contracts;
using CoreService.Api.Core.Models;
using CoreService.Api.Services;
using Shared.Access;

/// <summary>
/// Endpoint filters with the access rules. A filter checks the caller and, where a user may edit only a part
/// of a record, restores the protected fields from the stored values before the handler runs.
/// </summary>
public static class AccessFilters
{
    /// <summary>
    /// Listing every practice is for managers and reviewers.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeList(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter((context, next) =>
            PracticeAccess.CanListAll(context.HttpContext.User) ? next(context) : ForbiddenResult());

    /// <summary>
    /// A user may ask for the own data by user id, managers (and reviewers for practices) for anybody's.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <param name="reviewersToo">True if reviewers may read other people's records.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireOwnUserId(this RouteHandlerBuilder builder, bool reviewersToo = false)
        => builder.AddEndpointFilter((context, next) =>
        {
            var user = context.HttpContext.User;
            var requested = context.Arguments.OfType<string>().FirstOrDefault();
            var allowed = requested == user.UserId() || user.IsManager() || (reviewersToo && user.HasRole(UserRoleType.Reviewer));
            return allowed ? next(context) : ForbiddenResult();
        });

    /// <summary>
    /// Reading one practice, the route has the practice id.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeRead(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var practice = await Access(context).GetPracticeAsync(context.Arguments.OfType<int>().First());
            if (practice is null)
            {
                return Results.NotFound();
            }

            return PracticeAccess.CanRead(context.HttpContext.User, practice.Participants) ? await next(context) : Results.Forbid();
        });

    /// <summary>
    /// Creating a practice: a student creates the own one, managers any.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeCreate(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            var practice = context.Arguments.OfType<Practice>().First();
            if (!user.IsManager())
            {
                if (await Access(context).GetStudentUserIdAsync(practice.Studentid) != user.UserId())
                {
                    return Results.Forbid();
                }

                // The grade is given later by the supervisor.
                practice.Finalgrade = string.Empty;
            }

            return await next(context);
        });

    /// <summary>
    /// Updating a practice: students and supervisors change the plan, only the supervisor and managers the grade.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeUpdate(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            var practice = context.Arguments.OfType<Practice>().First();
            var current = await Access(context).GetPracticeAsync(practice.Id);
            if (current is null)
            {
                return Results.NotFound();
            }

            if (!PracticeAccess.CanEdit(user, current.Participants))
            {
                return Results.Forbid();
            }

            if (!PracticeAccess.CanGrade(user, current.Participants))
            {
                practice.Finalgrade = current.FinalGrade;
            }

            if (!user.IsManager())
            {
                practice.Studentid = current.StudentId;
            }

            return await next(context);
        });

    /// <summary>
    /// Deleting a practice, the route has the practice id.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeDelete(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var practice = await Access(context).GetPracticeAsync(context.Arguments.OfType<int>().First());
            if (practice is null)
            {
                return Results.NotFound();
            }

            return PracticeAccess.CanDelete(context.HttpContext.User, practice.Participants) ? await next(context) : Results.Forbid();
        });

    /// <summary>
    /// A new theme belongs to the user who creates it.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireThemeAuthorOnCreate(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter((context, next) =>
        {
            var user = context.HttpContext.User;
            var theme = context.Arguments.OfType<Theme>().First();
            if (!user.IsManager() || string.IsNullOrEmpty(theme.Suggestedby))
            {
                theme.Suggestedby = user.UserId() ?? string.Empty;
            }

            return next(context);
        });

    /// <summary>
    /// A theme may be changed, archived or deleted by the author and by managers.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireThemeAuthor(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            var theme = context.Arguments.OfType<Theme>().FirstOrDefault();
            var themeId = theme?.Id ?? context.Arguments.OfType<int>().First();
            var author = await Access(context).GetThemeAuthorAsync(themeId);
            if (author is null)
            {
                return Results.NotFound();
            }

            if (!user.IsManager() && author != user.UserId())
            {
                return Results.Forbid();
            }

            if (theme is not null && !user.IsManager())
            {
                theme.Suggestedby = author;
            }

            return await next(context);
        });

    /// <summary>
    /// A student record is edited by the student and by administrators, the link to the user stays as it is.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireStudentSelfOrAdmin(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            var student = context.Arguments.OfType<Student>().First();
            if (!user.HasRole(UserRoleType.Administrator))
            {
                var owner = await Access(context).GetStudentUserIdAsync(student.Id);
                if (owner is null || owner != user.UserId())
                {
                    return Results.Forbid();
                }

                student.Userid = owner;
            }

            return await next(context);
        });

    /// <summary>
    /// A lecturer record is edited by the lecturer and by administrators, the link to the user and the right to supervise stay as they are.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireLecturerSelfOrAdmin(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            var lecturer = context.Arguments.OfType<Lecturer>().First();
            if (!user.HasRole(UserRoleType.Administrator))
            {
                var current = await Access(context).GetLecturerAsync(lecturer.Id);
                if (current is null || current.Value.UserId != user.UserId())
                {
                    return Results.Forbid();
                }

                lecturer.Userid = current.Value.UserId;
                lecturer.Cansupervisevkr = current.Value.CanSuperviseVkr;
            }

            return await next(context);
        });

    /// <summary>
    /// A consultant record is edited by the consultant and by administrators, the link to the user stays as it is.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireConsultantSelfOrAdmin(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            var consultant = context.Arguments.OfType<Consultant>().First();
            if (!user.HasRole(UserRoleType.Administrator))
            {
                var owner = await Access(context).GetConsultantUserIdAsync(consultant.Id);
                if (owner is null || owner != user.UserId())
                {
                    return Results.Forbid();
                }

                consultant.Userid = owner;
            }

            return await next(context);
        });

    private static CoreAccess Access(EndpointFilterInvocationContext context)
        => context.HttpContext.RequestServices.GetRequiredService<CoreAccess>();

    private static ValueTask<object?> ForbiddenResult() => ValueTask.FromResult<object?>(Results.Forbid());
}
