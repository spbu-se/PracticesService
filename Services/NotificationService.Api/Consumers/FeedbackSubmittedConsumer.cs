// <copyright file="FeedbackSubmittedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for feedback submitted events.
/// </summary>
public class FeedbackSubmittedConsumer : IConsumer<FeedbackSubmittedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<FeedbackSubmittedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackSubmittedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public FeedbackSubmittedConsumer(
        IEmailService emailService,
        ILogger<FeedbackSubmittedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<FeedbackSubmittedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.SubmittedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Feedback submitted. PracticeId: {PracticeId}, Student: {StudentEmail}, Type: {FeedbackType}",
            message.PracticeId,
            message.StudentEmail,
            message.FeedbackType);

        try
        {
            // Notify student
            await this.emailService.SendEmailAsync(new SendEmailDto
            {
                To = message.StudentEmail,
                Subject = $"Новый отзыв по практике {practiceDisplay}",
                Body = $@"
                    <h2>Новый отзыв</h2>
                    <p>Был добавлен новый отзыв по практике {practiceDisplay}.</p>
                    <p><strong>Тип отзыва:</strong> {message.FeedbackType}</p>
                    <p><strong>Файл:</strong> {message.FileName}</p>
                    <p><strong>Дата:</strong> {formattedDate}</p>
                ",
                IsHtml = true,
            });

            this.logger.LogInformation(
                "Notification sent to student {StudentEmail} for practice {PracticeId}",
                message.StudentEmail,
                message.PracticeId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for feedback. PracticeId: {PracticeId}",
                message.PracticeId);
            throw;
        }
    }
}