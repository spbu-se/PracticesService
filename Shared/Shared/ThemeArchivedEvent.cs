// <copyright file="ThemeArchivedEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a theme is archived or unarchived.
/// </summary>
/// <param name="ThemeId">The ID of the theme.</param>
/// <param name="ThemeName">The name of the theme.</param>
/// <param name="IsArchived">The new archived status.</param>
/// <param name="UserId">The ID of the user who suggested the theme.</param>
/// <param name="UserEmail">The email of the user who suggested the theme.</param>
/// <param name="ActionDateTime">The timestamp of the action.</param>
public record ThemeArchivedEvent(
    int ThemeId,
    string ThemeName,
    bool IsArchived,
    string? UserId,
    string? UserEmail,
    DateTime ActionDateTime);