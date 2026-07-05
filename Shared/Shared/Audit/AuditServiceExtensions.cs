// <copyright file="AuditServiceExtensions.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Shared.Audit;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for audit service.
/// </summary>
public static class AuditServiceExtensions
{
    /// <summary>
    /// Adds audit service to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddAuditService(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}