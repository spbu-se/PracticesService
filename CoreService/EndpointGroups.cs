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
}
