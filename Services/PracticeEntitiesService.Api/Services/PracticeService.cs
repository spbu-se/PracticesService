// <copyright file="PracticeService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace PracticeEntities.Services;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PracticeEntities.Models;

/// <summary>
/// Service for resolving practice information.
/// </summary>
public class PracticeService
{
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PracticeService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public PracticeService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Gets practice details by practice ID.
    /// </summary>
    /// <param name="practiceId">The practice ID.</param>
    /// <returns>The practice details or null if not found.</returns>
    public async Task<PracticeDetails?> GetPracticeAsync(int practiceId)
    {
        var client = this.httpClientFactory.CreateClient("CoreService");
        var response = await client.GetAsync($"api/practices/{practiceId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PracticeDetails>();
    }

    /// <summary>
    /// Gets student email by practice ID.
    /// </summary>
    /// <param name="practiceId">The practice ID.</param>
    /// <returns>The student email or null if not found.</returns>
    public async Task<string?> GetStudentEmailAsync(int practiceId)
    {
        var practice = await this.GetPracticeAsync(practiceId);
        return practice?.Student?.Email;
    }

    /// <summary>
    /// Gets supervisor email by practice ID.
    /// </summary>
    /// <param name="practiceId">The practice ID.</param>
    /// <returns>The supervisor email or null if not found.</returns>
    public async Task<string?> GetSupervisorEmailAsync(int practiceId)
    {
        var practice = await this.GetPracticeAsync(practiceId);
        return practice?.Supervisor?.Email;
    }

    /// <summary>
    /// Gets both student and supervisor emails by practice ID.
    /// </summary>
    /// <param name="practiceId">The practice ID.</param>
    /// <returns>A tuple containing student and supervisor emails.</returns>
    public async Task<(string? StudentEmail, string? SupervisorEmail)> GetPracticeEmailsAsync(int practiceId)
    {
        var practice = await this.GetPracticeAsync(practiceId);
        return (practice?.Student?.Email, practice?.Supervisor?.Email);
    }
}