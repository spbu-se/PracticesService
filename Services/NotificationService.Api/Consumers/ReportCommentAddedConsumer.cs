// <copyright file="ReportCommentAddedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for report comment added events.
/// </summary>
public class ReportCommentAddedConsumer : IConsumer<ReportCommentAddedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<ReportCommentAddedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportCommentAddedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public ReportCommentAddedConsumer(
        IEmailService emailService,
        ILogger<ReportCommentAddedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ReportCommentAddedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.CreatedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Comment added to report. PracticeId: {PracticeId}, Author: {Author}, ReportId: {ReportId}",
            message.PracticeId,
            message.Author,
            message.ReportId);

        try
        {
            // Notify student (if author is not student)
            if (!string.IsNullOrEmpty(message.StudentEmail) && message.Author != "Student")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.StudentEmail,
                    Subject = $"Новый комментарий к отчету {practiceDisplay}",
                    Body = $@"
                        <h2>Новый комментарий</h2>
                        <p>Был добавлен новый комментарий к вашему отчету по практике {practiceDisplay}.</p>
                        <p><strong>Автор:</strong> {message.Author}</p>
                        <p><strong>Комментарий:</strong> {message.CommentText}</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to student {StudentEmail} for report {ReportId}",
                    message.StudentEmail,
                    message.ReportId);
            }

            // Notify supervisor (if author is not supervisor)
            if (!string.IsNullOrEmpty(message.SupervisorEmail) && message.Author != "Supervisor")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = $"Новый комментарий к отчету {practiceDisplay}",
                    Body = $@"
                        <h2>Новый комментарий</h2>
                        <p>Был добавлен новый комментарий к отчету студента по практике {practiceDisplay}.</p>
                        <p><strong>Автор:</strong> {message.Author}</p>
                        <p><strong>Комментарий:</strong> {message.CommentText}</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to supervisor {SupervisorEmail} for report {ReportId}",
                    message.SupervisorEmail,
                    message.ReportId);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for comment on report {ReportId}",
                message.ReportId);
            throw;
        }
    }
}