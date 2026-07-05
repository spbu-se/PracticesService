// <copyright file="ReportSubmittedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for report submitted events.
/// </summary>
public class ReportSubmittedConsumer : IConsumer<ReportSubmittedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<ReportSubmittedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSubmittedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public ReportSubmittedConsumer(
        IEmailService emailService,
        ILogger<ReportSubmittedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ReportSubmittedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.SubmittedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Report submitted. PracticeId: {PracticeId}",
            message.PracticeId);

        try
        {
            // Notify supervisor
            if (!string.IsNullOrEmpty(message.SupervisorEmail))
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = $"Новый отчет по практике {practiceDisplay}",
                    Body = $@"
                        <h2>Новый отчет</h2>
                        <p>Студент загрузил новый отчет по практике {practiceDisplay}.</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to supervisor {SupervisorEmail} for practice {PracticeId}",
                    message.SupervisorEmail,
                    message.PracticeId);
            }

            // Notify student
            if (!string.IsNullOrEmpty(message.StudentEmail))
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.StudentEmail,
                    Subject = $"Отчет по практике {practiceDisplay} сохранен",
                    Body = $@"
                        <h2>Отчет сохранен</h2>
                        <p>Ваш отчет по практике {practiceDisplay} был сохранен.</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to student {StudentEmail} for practice {PracticeId}",
                    message.StudentEmail,
                    message.PracticeId);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for report. PracticeId: {PracticeId}",
                message.PracticeId);
            throw;
        }
    }
}