// <copyright file="UserRoleType.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// User roles enum.
/// </summary>
public enum UserRoleType
{
    /// <summary>
    /// Student enum.
    /// </summary>
    Student = 1,

    /// <summary>
    /// Supervisor enum.
    /// </summary>
    Supervisor,

    /// <summary>
    /// Consultant enum.
    /// </summary>
    Consultant,

    /// <summary>
    /// Practice Leader enum.
    /// </summary>
    PracticeLeader,

    /// <summary>
    /// Reviewer enum.
    /// </summary>
    Reviewer,

    /// <summary>
    /// Administrator enum.
    /// </summary>
    Administrator,
}
