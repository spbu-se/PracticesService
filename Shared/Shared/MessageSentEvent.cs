// <copyright file="MessageSentEvent.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace Contracts;

/// <summary>
/// Event raised when a message is sent.
/// </summary>
/// <param name="PracticeId">The ID of the practice.</param>
/// <param name="PracticeTitle">The title of the practice.</param>
/// <param name="StudentEmail">The email of the student.</param>
/// <param name="SupervisorEmail">The email of the supervisor.</param>
/// <param name="Sender">The sender of the message.</param>
/// <param name="MessageText">The text content of the message.</param>
/// <param name="SentAt">The timestamp when the message was sent.</param>
public record MessageSentEvent(
    int PracticeId,
    string PracticeTitle,
    string? StudentEmail,
    string? SupervisorEmail,
    string Sender,
    string MessageText,
    DateTime SentAt);