// <copyright file="IEmailService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Interfaces;

using NotificationService.Api.Models.DTOs;

/// <summary>
/// Interface for email service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="emailDto">Email data.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendEmailAsync(SendEmailDto emailDto);

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <param name="request">Password reset request data.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendPasswordResetEmailAsync(PasswordResetRequestDto request);

    /// <summary>
    /// Sends an email confirmation email.
    /// </summary>
    /// <param name="request">Email confirmation request data.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendEmailConfirmationAsync(EmailConfirmationRequestDto request);
}