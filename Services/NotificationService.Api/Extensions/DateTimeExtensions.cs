// <copyright file="DateTimeExtensions.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace NotificationService.Api.Extensions;

/// <summary>
/// Extension methods for DateTime.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts UTC DateTime to Moscow time.
    /// </summary>
    /// <param name="utcDateTime">UTC DateTime.</param>
    /// <returns>Moscow local time.</returns>
    public static DateTime ToMoscowTime(this DateTime utcDateTime)
    {
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, moscowTimeZone);
    }

    /// <summary>
    /// Formats DateTime to string with Moscow time.
    /// </summary>
    /// <param name="utcDateTime">UTC DateTime.</param>
    /// <param name="format">Format string.</param>
    /// <returns>Formatted date string.</returns>
    public static string ToMoscowTimeString(this DateTime utcDateTime, string format = "dd.MM.yyyy HH:mm")
    {
        return utcDateTime.ToMoscowTime().ToString(format);
    }
}