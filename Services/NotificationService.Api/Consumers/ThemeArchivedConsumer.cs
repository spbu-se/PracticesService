// <copyright file="ThemeArchivedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for theme archived events.
/// </summary>
public class ThemeArchivedConsumer : IConsumer<ThemeArchivedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<ThemeArchivedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeArchivedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public ThemeArchivedConsumer(
        IEmailService emailService,
        ILogger<ThemeArchivedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ThemeArchivedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        this.logger.LogInformation(
            "Processing theme archive event. CorrelationId: {CorrelationId}, ThemeId: {ThemeId}, IsArchived: {IsArchived}, Email: {Email}",
            correlationId,
            message.ThemeId,
            message.IsArchived,
            message.UserEmail ?? "unknown");

        try
        {
            if (!string.IsNullOrEmpty(message.UserEmail))
            {
                var subject = message.IsArchived
                    ? "Ваша тема была заархивирована"
                    : "Ваша тема была разархивирована";

                var body = $@"
                    <h2>{subject}</h2>
                    <p>Здравствуйте!</p>
                    <p>Тема <strong>'{message.ThemeName}'</strong> была {(message.IsArchived ? "заархивирована" : "разархивирована")}.</p>
                ";

                var emailDto = new SendEmailDto
                {
                    To = message.UserEmail,
                    Subject = subject,
                    Body = body,
                    IsHtml = true,
                };

                await this.emailService.SendEmailAsync(emailDto);

                this.logger.LogInformation(
                    "Theme archive notification sent to {Email}. CorrelationId: {CorrelationId}",
                    message.UserEmail,
                    correlationId);
            }
            else
            {
                this.logger.LogWarning(
                    "No user email provided for theme archive notification. CorrelationId: {CorrelationId}, ThemeId: {ThemeId}",
                    correlationId,
                    message.ThemeId);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error processing theme archive event. CorrelationId: {CorrelationId}, ThemeId: {ThemeId}",
                correlationId,
                message.ThemeId);
            throw;
        }
    }
}