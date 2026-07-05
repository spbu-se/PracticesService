// <copyright file="PresentationCommentAddedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for presentation comment added events.
/// </summary>
public class PresentationCommentAddedConsumer : IConsumer<PresentationCommentAddedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<PresentationCommentAddedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PresentationCommentAddedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public PresentationCommentAddedConsumer(
        IEmailService emailService,
        ILogger<PresentationCommentAddedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<PresentationCommentAddedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.CreatedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Comment added to presentation. PracticeId: {PracticeId}, Author: {Author}, PresentationId: {PresentationId}",
            message.PracticeId,
            message.Author,
            message.PresentationId);

        try
        {
            // Notify student (if author is not student)
            if (!string.IsNullOrEmpty(message.StudentEmail) && message.Author != "Student")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.StudentEmail,
                    Subject = $"Новый комментарий к презентации {practiceDisplay}",
                    Body = $@"
                        <h2>Новый комментарий</h2>
                        <p>Был добавлен новый комментарий к вашей презентации {practiceDisplay}.</p>
                        <p><strong>Файл:</strong> {message.FileName}</p>
                        <p><strong>Версия:</strong> {message.Version}</p>
                        <p><strong>Автор:</strong> {message.Author}</p>
                        <p><strong>Комментарий:</strong> {message.CommentText}</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to student {StudentEmail} for presentation {PresentationId}",
                    message.StudentEmail,
                    message.PresentationId);
            }

            // Notify supervisor (if author is not supervisor)
            if (!string.IsNullOrEmpty(message.SupervisorEmail) && message.Author != "Supervisor")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = $"Новый комментарий к презентации {practiceDisplay}",
                    Body = $@"
                        <h2>Новый комментарий</h2>
                        <p>Был добавлен новый комментарий к презентации студента {practiceDisplay}.</p>
                        <p><strong>Файл:</strong> {message.FileName}</p>
                        <p><strong>Версия:</strong> {message.Version}</p>
                        <p><strong>Автор:</strong> {message.Author}</p>
                        <p><strong>Комментарий:</strong> {message.CommentText}</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to supervisor {SupervisorEmail} for presentation {PresentationId}",
                    message.SupervisorEmail,
                    message.PresentationId);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for comment on presentation {PresentationId}",
                message.PresentationId);
            throw;
        }
    }
}