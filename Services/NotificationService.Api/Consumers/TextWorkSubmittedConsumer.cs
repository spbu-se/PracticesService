// <copyright file="TextWorkSubmittedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for text work submitted events.
/// </summary>
public class TextWorkSubmittedConsumer : IConsumer<TextWorkSubmittedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<TextWorkSubmittedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextWorkSubmittedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public TextWorkSubmittedConsumer(
        IEmailService emailService,
        ILogger<TextWorkSubmittedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<TextWorkSubmittedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.SubmittedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Text work submitted. PracticeId: {PracticeId}, Student: {StudentEmail}, Supervisor: {SupervisorEmail}, Version: {Version}",
            message.PracticeId,
            message.StudentEmail,
            message.SupervisorEmail,
            message.Version);

        try
        {
            // Notify supervisor
            await this.emailService.SendEmailAsync(new SendEmailDto
            {
                To = message.SupervisorEmail,
                Subject = $"Новый текст работы по практике {practiceDisplay}",
                Body = $@"
                    <h2>Новый текст работы</h2>
                    <p>Студент загрузил новый текст работы по практике {practiceDisplay}.</p>
                    <p><strong>Студент:</strong> {message.StudentEmail}</p>
                    <p><strong>Файл:</strong> {message.FileName}</p>
                    <p><strong>Версия:</strong> {message.Version}</p>
                    <p><strong>Дата:</strong> {formattedDate}</p>
                ",
                IsHtml = true,
            });

            this.logger.LogInformation(
                "Notification sent to supervisor {SupervisorEmail} for practice {PracticeId}",
                message.SupervisorEmail,
                message.PracticeId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for text work. PracticeId: {PracticeId}",
                message.PracticeId);
            throw;
        }
    }
}