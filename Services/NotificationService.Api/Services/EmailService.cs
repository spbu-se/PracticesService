// <copyright file="EmailService.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;
using NotificationService.Api.Models.DTOs;

/// <summary>
/// Service for handling email operations.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings smtpSettings;
    private readonly ILogger<EmailService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="smtpSettings">SMTP configuration settings.</param>
    /// <param name="logger">Logger instance for email operations.</param>
    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        this.smtpSettings = smtpSettings.Value;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> SendEmailAsync(SendEmailDto emailDto)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(this.smtpSettings.FromName, this.smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(string.Empty, emailDto.To));
            message.Subject = emailDto.Subject;

            var bodyBuilder = new BodyBuilder();
            if (emailDto.IsHtml)
            {
                bodyBuilder.HtmlBody = emailDto.Body;
            }
            else
            {
                bodyBuilder.TextBody = emailDto.Body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            client.ServerCertificateValidationCallback = (_, _, _, _) => true;

            var secureOption = this.smtpSettings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(this.smtpSettings.Host, this.smtpSettings.Port, secureOption).ConfigureAwait(false);

            await client.AuthenticateAsync(this.smtpSettings.Username, this.smtpSettings.Password).ConfigureAwait(false);
            await client.SendAsync(message).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);

            this.logger.LogInformation("Email sent successfully to {To}", emailDto.To);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send email to {To}. Error: {Message}", emailDto.To, ex.Message);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SendPasswordResetEmailAsync(PasswordResetRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Восстановление пароля</h2>
                <p>Здравствуйте, {request.UserName}!</p>
                <p>Для восстановления пароля перейдите по ссылке:</p>
                <p>
                    <a href='{request.ResetLink}'
                       style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>
                        Восстановить пароль
                    </a>
                </p>
                <p>Ссылка действительна в течение 24 часов.</p>
                <p>Если вы не запрашивали восстановление пароля, проигнорируйте это письмо.</p>
                <br/>
                <p>С уважением,<br/>Команда системы практик</p>
            </div>";

        var emailDto = new SendEmailDto
        {
            To = request.Email,
            Subject = "Восстановление пароля - Система практик",
            Body = body,
            IsHtml = true,
        };

        return await this.SendEmailAsync(emailDto).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> SendEmailConfirmationAsync(EmailConfirmationRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Добро пожаловать, {request.UserName}!</h2>
                <p>Пожалуйста, подтвердите ваш email, перейдя по ссылке:</p>
                <p>
                    <a href='{request.ConfirmLink}'
                       style='padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>
                        Подтвердить email
                    </a>
                </p>
                <p>Ссылка действительна в течение 24 часов.</p>
                <p>Если вы не регистрировались в системе, проигнорируйте это письмо.</p>
                <br/>
                <p>С уважением,<br/>Команда системы практик</p>
            </div>";

        var emailDto = new SendEmailDto
        {
            To = request.Email,
            Subject = "Подтверждение email - Система практик",
            Body = body,
            IsHtml = true,
        };

        return await this.SendEmailAsync(emailDto).ConfigureAwait(false);
    }
}