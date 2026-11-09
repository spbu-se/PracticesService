// <copyright file="CallerExtensions.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Access;

using System.Security.Claims;
using Contracts;

/// <summary>
/// Helpers to read the signed-in user from the token claims.
/// </summary>
public static class CallerExtensions
{
    /// <summary>
    /// Gets the user id (the auth service id) of the caller.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <returns>The id or null if the caller is anonymous.</returns>
    public static string? UserId(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Checks whether the caller has the role.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <param name="role">Role to check.</param>
    /// <returns>True if the caller has the role.</returns>
    public static bool HasRole(this ClaimsPrincipal user, UserRoleType role) => user.IsInRole(RoleNames.GetName(role));

    /// <summary>
    /// Checks whether the call comes from another service of the system (it proved itself with the shared key).
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <returns>True for service to service calls.</returns>
    public static bool IsInternalService(this ClaimsPrincipal user) => user.Identity?.AuthenticationType == "InternalKey";

    /// <summary>
    /// Administrators and practice leaders manage everything.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <returns>True for administrators and practice leaders.</returns>
    public static bool IsManager(this ClaimsPrincipal user)
        => user.HasRole(UserRoleType.Administrator) || user.HasRole(UserRoleType.PracticeLeader);

    /// <summary>
    /// Gets the name as the user interface shows it: last, first and middle name.
    /// </summary>
    /// <param name="user">The caller.</param>
    /// <returns>The full name or the login if the claims have no name.</returns>
    public static string DisplayName(this ClaimsPrincipal user)
    {
        var parts = new[]
        {
            user.FindFirst(ClaimTypes.Surname)?.Value,
            user.FindFirst(ClaimTypes.GivenName)?.Value,
            user.FindFirst("middle_name")?.Value,
        }.Where(p => !string.IsNullOrWhiteSpace(p));

        var name = string.Join(' ', parts);
        return name.Length > 0 ? name : user.Identity?.Name ?? string.Empty;
    }
}
