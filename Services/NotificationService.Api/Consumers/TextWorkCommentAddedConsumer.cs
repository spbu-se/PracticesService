// <copyright file="TextWorkCommentAddedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for text work comment added events.
/// </summary>
public class TextWorkCommentAddedConsumer : IConsumer<TextWorkCommentAddedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<TextWorkCommentAddedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextWorkCommentAddedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public TextWorkCommentAddedConsumer(
        IEmailService emailService,
        ILogger<TextWorkCommentAddedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<TextWorkCommentAddedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.CreatedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Comment added to text work. PracticeId: {PracticeId}, Author: {Author}, TextWorkId: {TextWorkId}",
            message.PracticeId,
            message.Author,
            message.TextWorkId);

        try
        {
            // Notify student (if author is not student)
            if (!string.IsNullOrEmpty(message.StudentEmail) && message.Author != "Student")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.StudentEmail,
                    Subject = $"Новый комментарий к тексту работы {practiceDisplay}",
                    Body = $@"
                        <h2>Новый комментарий</h2>
                        <p>Был добавлен новый комментарий к вашей тексту работы {practiceDisplay}.</p>
                        <p><strong>Файл:</strong> {message.FileName}</p>
                        <p><strong>Версия:</strong> {message.Version}</p>
                        <p><strong>Автор:</strong> {message.Author}</p>
                        <p><strong>Комментарий:</strong> {message.CommentText}</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to student {StudentEmail} for text work {TextWorkId}",
                    message.StudentEmail,
                    message.TextWorkId);
            }

            // Notify supervisor (if author is not supervisor)
            if (!string.IsNullOrEmpty(message.SupervisorEmail) && message.Author != "Supervisor")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = $"Новый комментарий к тексту работы {practiceDisplay}",
                    Body = $@"
                        <h2>Новый комментарий</h2>
                        <p>Был добавлен новый комментарий к тексту работы студента {practiceDisplay}.</p>
                        <p><strong>Файл:</strong> {message.FileName}</p>
                        <p><strong>Версия:</strong> {message.Version}</p>
                        <p><strong>Автор:</strong> {message.Author}</p>
                        <p><strong>Комментарий:</strong> {message.CommentText}</p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to supervisor {SupervisorEmail} for text work {TextWorkId}",
                    message.SupervisorEmail,
                    message.TextWorkId);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for comment on text work {TextWorkId}",
                message.TextWorkId);
            throw;
        }
    }
}