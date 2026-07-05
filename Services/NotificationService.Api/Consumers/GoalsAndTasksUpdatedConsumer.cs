// <copyright file="GoalsAndTasksUpdatedConsumer.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Consumers;

using Contracts;
using MassTransit;
using NotificationService.Api.Extensions;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Consumer for goals and tasks updated events.
/// </summary>
public class GoalsAndTasksUpdatedConsumer : IConsumer<GoalsAndTasksUpdatedEvent>
{
    private readonly IEmailService emailService;
    private readonly ILogger<GoalsAndTasksUpdatedConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalsAndTasksUpdatedConsumer"/> class.
    /// </summary>
    /// <param name="emailService">Email service.</param>
    /// <param name="logger">Logger instance.</param>
    public GoalsAndTasksUpdatedConsumer(
        IEmailService emailService,
        ILogger<GoalsAndTasksUpdatedConsumer> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<GoalsAndTasksUpdatedEvent> context)
    {
        var message = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        var practiceDisplay = !string.IsNullOrEmpty(message.PracticeTitle)
            ? $"'{message.PracticeTitle}'"
            : $"#{message.PracticeId}";

        var formattedDate = message.UpdatedAt.ToMoscowTimeString("dd.MM.yyyy HH:mm");

        this.logger.LogInformation(
            "Goals and tasks updated. PracticeId: {PracticeId}, PracticeTitle: {PracticeTitle}, Student: {StudentEmail}",
            message.PracticeId,
            message.PracticeTitle,
            message.StudentEmail);

        try
        {
            // Notify student
            await this.emailService.SendEmailAsync(new SendEmailDto
            {
                To = message.StudentEmail,
                Subject = "Цели и задачи обновлены",
                Body = $@"
                    <h2>Цели и задачи обновлены</h2>
                    <p>Цели и задачи для практики {practiceDisplay} были обновлены.</p>
                    <p>Дата: {formattedDate}</p>
                ",
                IsHtml = true,
            });

            // Notify supervisor if exists
            if (!string.IsNullOrEmpty(message.SupervisorEmail))
            {
                await this.emailService.SendEmailAsync(new SendEmailDto
                {
                    To = message.SupervisorEmail,
                    Subject = "Цели и задачи обновлены",
                    Body = $@"
                        <h2>Цели и задачи обновлены</h2>
                        <p>Цели и задачи для практики {practiceDisplay} были обновлены.</p>
                        <p>Дата: {formattedDate}</p>
                    ",
                    IsHtml = true,
                });
            }

            this.logger.LogInformation(
                "Notifications sent for practice {PracticeId} ({PracticeTitle})",
                message.PracticeId,
                message.PracticeTitle);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error sending notifications for practice {PracticeId}",
                message.PracticeId);
            throw;
        }
    }
}