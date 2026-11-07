namespace CincyEvents.Services;

public static class EventTimeFormatter
{
    public static string Format(DateTime startAtUtc, string? timeZoneId)
    {
        var utcStart = DateTime.SpecifyKind(startAtUtc, DateTimeKind.Utc);
        var timeZone = string.IsNullOrWhiteSpace(timeZoneId)
            ? TimeZoneInfo.Utc
            : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(utcStart, timeZone);
        var zoneName = timeZone.IsDaylightSavingTime(localStart)
            ? timeZone.DaylightName
            : timeZone.StandardName;
        return $"{localStart:f} ({zoneName})";
    }
}
