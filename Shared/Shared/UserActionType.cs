// <copyright file="UserActionType.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Enum for actions.
/// </summary>
public enum UserActionType
{
    /// <summary>
    /// Enum for Create.
    /// </summary>
    Create = 1,

    /// <summary>
    /// Enum for Update.
    /// </summary>
    Update = 2,

    /// <summary>
    /// Enum for Delete.
    /// </summary>
    Delete = 3,
}