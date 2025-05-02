// <copyright file="AuthResponse.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace AuthService.Api.Models;

/// <summary>
/// Auth response class.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Gets or sets Token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Refresh Token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}