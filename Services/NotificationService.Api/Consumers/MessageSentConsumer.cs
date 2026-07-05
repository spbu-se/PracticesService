// <copyright file="MessageSentConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for message sent events.
/// </summary>
public class MessageSentConsumer : IConsumer<MessageSentEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<MessageSentConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSentConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public MessageSentConsumer(
        IEmailService emailService,
        ILogger<MessageSentConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<MessageSentEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.SentAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Message sent. PracticeId: {PracticeId}, Sender: {Sender}",
            message.PracticeId,
            message.Sender);

        try
        {
            // Notify student (if sender is not student)
            if (!string.IsNullOrEmpty(message.StudentEmail) && message.Sender != "Student")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.StudentEmail,
                    Subject = $"Новое сообщение по практике {practiceDisplay}",
                    Body = $@"
                        <h2>Новое сообщение</h2>
                        <p>Вы получили новое сообщение по практике {practiceDisplay}.</p>
                        <p><strong>От:</strong> {message.Sender}</p>
                        <p><strong>Сообщение:</strong></p>
                        <p style='padding: 10px; background-color: #f5f5f5; border-radius: 5px;'>
                            {message.MessageText}
                        </p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                        <p><em>Для ответа перейдите в систему.</em></p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to student {StudentEmail} for practice {PracticeId}",
                    message.StudentEmail,
                    message.PracticeId);
            }

            // Notify supervisor (if sender is not supervisor)
            if (!string.IsNullOrEmpty(message.SupervisorEmail) && message.Sender != "Supervisor")
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = $"Новое сообщение по практике {practiceDisplay}",
                    Body = $@"
                        <h2>Новое сообщение</h2>
                        <p>Вы получили новое сообщение по практике {practiceDisplay}.</p>
                        <p><strong>От:</strong> {message.Sender}</p>
                        <p><strong>Сообщение:</strong></p>
                        <p style='padding: 10px; background-color: #f5f5f5; border-radius: 5px;'>
                            {message.MessageText}
                        </p>
                        <p><strong>Дата:</strong> {formattedDate}</p>
                        <p><em>Для ответа перейдите в систему.</em></p>
                    ",
                    IsHtml = true,
                });

                this.logger.LogInformation(
                    "Notification sent to supervisor {SupervisorEmail} for practice {PracticeId}",
                    message.SupervisorEmail,
                    message.PracticeId);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notification for message. PracticeId: {PracticeId}",
                message.PracticeId);
            throw;
        }
    }
}