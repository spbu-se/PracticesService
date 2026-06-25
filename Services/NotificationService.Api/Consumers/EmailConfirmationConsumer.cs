// <copyright file="EmailConfirmationConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for email confirmation requests.
/// </summary>
public class EmailConfirmationConsumer : IConsumer<EmailConfirmationRequestedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<EmailConfirmationConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfirmationConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public EmailConfirmationConsumer(
        IEmailService emailService,
        ILogger<EmailConfirmationConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<EmailConfirmationRequestedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        this.logger.LogInformation(
            "Processing email confirmation. CorrelationId: {CorrelationId}, UserId: {UserId}, Email: {Email}",
            correlationId,
            message.UserId,
            message.Email);

        try
        {
            var request = new EmailConfirmationRequestDto
            {
                Email = message.Email,
                UserName = message.UserName,
                ConfirmLink = message.ConfirmLink,
            };

            var result = await this.emailService.SendEmailConfirmationAsync(request);

            if (result)
            {
                this.logger.LogInformation(
                    "Email confirmation sent successfully. CorrelationId: {CorrelationId}, Email: {Email}",
                    correlationId,
                    message.Email);
            }
            else
            {
                this.logger.LogWarning(
                    "Failed to send email confirmation. CorrelationId: {CorrelationId}, Email: {Email}",
                    correlationId,
                    message.Email);

                throw new InvalidOperationException($"Failed to send confirmation email to {message.Email}");
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error processing email confirmation. CorrelationId: {CorrelationId}, UserId: {UserId}, Email: {Email}",
                correlationId,
                message.UserId,
                message.Email);
            throw;
        }
    }
}