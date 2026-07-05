// <copyright file="PracticeUpdatedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for practice updated events.
/// </summary>
public class PracticeUpdatedConsumer : IConsumer<PracticeUpdatedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<PracticeUpdatedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PracticeUpdatedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public PracticeUpdatedConsumer(
        IEmailService emailService,
        ILogger<PracticeUpdatedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<PracticeUpdatedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        this.logger.LogInformation(
            "Processing practice updated event. CorrelationId: {CorrelationId}, PracticeId: {PracticeId}, StudentEmail: {StudentEmail}",
            correlationId,
            message.PracticeId,
            message.StudentEmail);

        try
        {
            // Send notification to student
            if (!string.IsNullOrEmpty(message.StudentEmail))
            {
                var studentSubject = "Обновление информации о практике";
                var studentBody = $@"
                    <h2>Обновление практики</h2>
                    <p>Здравствуйте!</p>
                    <p>Информация о вашей практике <strong>'{message.PracticeTitle}'</strong> была обновлена.</p>
                    <p>Для просмотра подробностей перейдите в личный кабинет.</p>
                ";

                var studentEmailDto = new SendEmailDto
                {
                    To = message.StudentEmail,
                    Subject = studentSubject,
                    Body = studentBody,
                    IsHtml = true,
                };

                await this.emailService.SendEmailAsync(studentEmailDto);

                this.logger.LogInformation(
                    "Notification sent to student {StudentEmail}. CorrelationId: {CorrelationId}",
                    message.StudentEmail,
                    correlationId);
            }

            // Send notification to supervisor if email exists
            if (!string.IsNullOrEmpty(message.SupervisorEmail))
            {
                var supervisorSubject = "Обновление практики студента";
                var supervisorBody = $@"
                    <h2>Обновление практики</h2>
                    <p>Здравствуйте!</p>
                    <p>Обновилась информация о практике <strong>'{message.PracticeTitle}'</strong>.</p>
                    <p>Для просмотра подробностей перейдите в систему.</p>
                ";

                var supervisorEmailDto = new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = supervisorSubject,
                    Body = supervisorBody,
                    IsHtml = true,
                };

                await this.emailService.SendEmailAsync(supervisorEmailDto);

                this.logger.LogInformation(
                    "Notification sent to supervisor {SupervisorEmail}. CorrelationId: {CorrelationId}",
                    message.SupervisorEmail,
                    correlationId);
            }

            this.logger.LogInformation(
                "Practice updated event processed successfully. CorrelationId: {CorrelationId}, PracticeId: {PracticeId}",
                correlationId,
                message.PracticeId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error processing practice updated event. CorrelationId: {CorrelationId}, PracticeId: {PracticeId}",
                correlationId,
                message.PracticeId);
            throw;
        }
    }
}