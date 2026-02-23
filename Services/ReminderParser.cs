using System.Globalization;
using System.Text.RegularExpressions;
using ReminderMe.Models;

namespace ReminderMe.Services;

public sealed class ReminderParser
{
    private static readonly Regex TimeRegex =
        new(@"\b(?<hour>\d{1,2})(:(?<minute>\d{2}))?\s*(?<ampm>am|pm)?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool TryParse(string input, DateTime now, out ReminderItem? reminder, out string error)
    {
        reminder = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Please type a reminder request.";
            return false;
        }

        var timeMatch = TimeRegex.Match(input);
        if (!timeMatch.Success)
        {
            error = "I couldn't find a time. Include something like 5 pm or 17:30.";
            return false;
        }

        if (!TryBuildTime(timeMatch, out var hour, out var minute))
        {
            error = "The time looks invalid. Try formats like 5 pm, 8:30 am, or 17:30.";
            return false;
        }

        var dueDate = now.Date;
        var normalized = input.ToLowerInvariant();
        if (normalized.Contains("tomorrow"))
        {
            dueDate = dueDate.AddDays(1);
        }

        var dueAt = dueDate.AddHours(hour).AddMinutes(minute);
        if (dueAt <= now)
        {
            dueAt = dueAt.AddDays(1);
        }

        var description = BuildDescription(input, timeMatch.Value);
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "Untitled reminder";
        }

        reminder = new ReminderItem
        {
            Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(description.Trim()),
            DueAt = dueAt
        };

        return true;
    }

    private static string BuildDescription(string input, string matchedTime)
    {
        var cleaned = input
            .Replace("hey myapp", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("can you please", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("remind me to", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("remind me", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("today", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("tomorrow", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("at", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(matchedTime, string.Empty, StringComparison.OrdinalIgnoreCase);

        return Regex.Replace(cleaned, @"\s+", " ").Trim(' ', ',', '.', '?', '!');
    }

    private static bool TryBuildTime(Match timeMatch, out int hour24, out int minute)
    {
        hour24 = 0;
        minute = 0;

        if (!int.TryParse(timeMatch.Groups["hour"].Value, out var hour))
        {
            return false;
        }

        if (timeMatch.Groups["minute"].Success &&
            !int.TryParse(timeMatch.Groups["minute"].Value, out minute))
        {
            return false;
        }

        if (minute is < 0 or > 59)
        {
            return false;
        }

        var ampm = timeMatch.Groups["ampm"].Value.ToLowerInvariant();
        if (!string.IsNullOrEmpty(ampm))
        {
            if (hour is < 1 or > 12)
            {
                return false;
            }

            hour24 = ampm switch
            {
                "am" when hour == 12 => 0,
                "am" => hour,
                "pm" when hour == 12 => 12,
                "pm" => hour + 12,
                _ => hour
            };
        }
        else
        {
            if (hour is < 0 or > 23)
            {
                return false;
            }

            hour24 = hour;
        }

        return true;
    }
}
