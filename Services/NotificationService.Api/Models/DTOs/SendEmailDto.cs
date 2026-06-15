// <copyright file="SendEmailDto.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Models.DTOs;

/// <summary>
/// Data transfer object for sending email.
/// </summary>
public class SendEmailDto
{
    /// <summary>
    /// Gets or sets the recipient email address.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the body is HTML.
    /// </summary>
    public bool IsHtml { get; set; } = true;
}