// <copyright file="PracticeRelation.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Access;

/// <summary>
/// How a user is related to a particular practice.
/// </summary>
public enum PracticeRelation
{
    /// <summary>
    /// The user is not a participant of the practice.
    /// </summary>
    None,

    /// <summary>
    /// The user is the student who does the practice.
    /// </summary>
    Student,

    /// <summary>
    /// The user is the scientific supervisor of the practice.
    /// </summary>
    Supervisor,

    /// <summary>
    /// The user is the consultant of the practice.
    /// </summary>
    Consultant,
}
