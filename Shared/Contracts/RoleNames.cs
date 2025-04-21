// <copyright file="RoleNames.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Role names.
/// </summary>
public static class RoleNames
{
    private static readonly Dictionary<UserRoleType, string> RoleNamesValue = new()
    {
        { UserRoleType.Student, "Студент" },
        { UserRoleType.Supervisor, "Научный руководитель" },
        { UserRoleType.Consultant, "Консультант" },
        { UserRoleType.PracticeLeader, "Руководитель практики" },
        { UserRoleType.Reviewer, "Рецензент" },
        { UserRoleType.Administrator, "Администратор" },
    };

    /// <summary>
    /// Get all roles names.
    /// </summary>
    /// <returns>Array of role names.</returns>
    public static string[] GetAllRoleNames() => RoleNamesValue.Values.ToArray();

    /// <summary>
    /// Get roel name.
    /// </summary>
    /// <param name="role">Role enum.</param>
    /// <returns>String name of role.</returns>
    public static string GetName(UserRoleType role)
        => RoleNamesValue.TryGetValue(role, out var name) ? name : role.ToString();
}