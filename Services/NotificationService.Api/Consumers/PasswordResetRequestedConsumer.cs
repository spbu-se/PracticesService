// <copyright file="PasswordResetRequestedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for password reset requested events.
/// </summary>
public class PasswordResetRequestedConsumer : IConsumer<PasswordResetRequestedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<PasswordResetRequestedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetRequestedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public PasswordResetRequestedConsumer(
        IEmailService emailService,
        ILogger<PasswordResetRequestedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<PasswordResetRequestedEvent> context)
    {
        var message = context.Message;

        this.logger.LogInformation("Sending password reset email to {Email}", message.Email);

        var request = new PasswordResetRequestDto
        {
            Email = message.Email,
            ResetLink = message.ResetLink,
            UserName = message.UserName,
        };

        await this.emailService.SendPasswordResetEmailAsync(request);
    }
}