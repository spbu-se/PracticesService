// <copyright file="PracticeAccessFilters.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Services;

using System.Security.Claims;
using MongoDB.Driver;
using PracticeEntities.Models;
using Shared.Access;

/// <summary>
/// Endpoint filters that apply the practice access rules to the data attached to a practice.
/// The practice itself is read from the core service, so the rules are the same as there.
/// </summary>
public static class PracticeAccessFilters
{
    /// <summary>
    /// Reading data of a practice, the route has the practice id.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeRead(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter((context, next) =>
            Guard(context, context.Arguments.OfType<int>().First(), PracticeAccess.CanRead, next));

    /// <summary>
    /// Adding reports, works, goals, repositories and messages. The author of a message is taken from the token.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePracticeContribute(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter((context, next) =>
        {
            if (context.Arguments.OfType<Message>().FirstOrDefault() is { } message)
            {
                message.Author = context.HttpContext.User.DisplayName();
            }

            return Guard(context, context.Arguments.OfType<IPracticeScoped>().First().PracticeId, PracticeAccess.CanContribute, next);
        });

    /// <summary>
    /// Uploading a feedback, it is written by the supervisor or the consultant.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireFeedbackAccess(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter((context, next) =>
            Guard(context, context.Arguments.OfType<IPracticeScoped>().First().PracticeId, PracticeAccess.CanLeaveFeedback, next));

    /// <summary>
    /// Commenting a text work, the route has the id of the work.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireTextWorkCommentAccess(this RouteHandlerBuilder builder)
        => builder.RequireCommentAccess(async (db, id) =>
            await db.GetCollection<TextWork>("textworks").Find(x => x.Id == id).Project(x => (int?)x.PracticeId).FirstOrDefaultAsync());

    /// <summary>
    /// Commenting a presentation, the route has the id of the presentation.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequirePresentationCommentAccess(this RouteHandlerBuilder builder)
        => builder.RequireCommentAccess(async (db, id) =>
            await db.GetCollection<Presentation>("presentations").Find(x => x.Id == id).Project(x => (int?)x.PracticeId).FirstOrDefaultAsync());

    /// <summary>
    /// Commenting a report, the route has the id of the report.
    /// </summary>
    /// <param name="builder">Endpoint builder.</param>
    /// <returns>The same builder.</returns>
    public static RouteHandlerBuilder RequireReportCommentAccess(this RouteHandlerBuilder builder)
        => builder.RequireCommentAccess(async (db, id) =>
            await db.GetCollection<Report>("reports").Find(x => x.Id == id).Project(x => (int?)x.PracticeId).FirstOrDefaultAsync());

    private static RouteHandlerBuilder RequireCommentAccess(
        this RouteHandlerBuilder builder,
        Func<IMongoDatabase, string, Task<int?>> practiceIdOf)
        => builder.AddEndpointFilter(async (context, next) =>
        {
            var id = context.Arguments.OfType<string>().First();
            var db = context.HttpContext.RequestServices.GetRequiredService<IMongoDatabase>();
            var practiceId = await practiceIdOf(db, id);
            if (practiceId is null)
            {
                return Results.NotFound();
            }

            if (context.Arguments.OfType<Comment>().FirstOrDefault() is { } comment)
            {
                comment.Author = context.HttpContext.User.DisplayName();
            }

            return await Guard(context, practiceId.Value, PracticeAccess.CanContribute, next);
        });

    private static async ValueTask<object?> Guard(
        EndpointFilterInvocationContext context,
        int practiceId,
        Func<ClaimsPrincipal, PracticeParticipants, bool> allowed,
        EndpointFilterDelegate next)
    {
        var practice = await context.HttpContext.RequestServices.GetRequiredService<PracticeService>().GetPracticeAsync(practiceId);
        if (practice is null)
        {
            return Results.NotFound();
        }

        var participants = new PracticeParticipants(practice.Student?.Userid, practice.Supervisor?.Userid, practice.Consultant?.Userid);
        return allowed(context.HttpContext.User, participants) ? await next(context) : Results.Forbid();
    }
}
