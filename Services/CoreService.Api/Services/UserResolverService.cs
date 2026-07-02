// <copyright file="UserResolverService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Api.Services;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contracts;

/// <summary>
/// Service for resolving user information.
/// </summary>
public class UserResolverService
{
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserResolverService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public UserResolverService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Gets user information by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user DTO or null if not found.</returns>
    public async Task<UserDTO?> GetUserAsync(Guid userId)
    {
        var client = this.httpClientFactory.CreateClient("AuthService");
        var response = await client.GetAsync($"users/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserDTO>();
    }

    /// <summary>
    /// Gets user ID by username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>The user ID as string or null if not found.</returns>
    public async Task<string?> GetUserIdAsync(string username)
    {
        var client = this.httpClientFactory.CreateClient("AuthService");
        var response = await client.GetAsync($"userId?userName={username}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }
}