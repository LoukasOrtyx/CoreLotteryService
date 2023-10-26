namespace CoreLotteryService.Library.Helpers;

/// <summary>
/// An utility class for handling <see cref="DateTime"/> under the current local 
/// <see cref="TimeZoneInfo"/>.
/// </summary
public static class DateTimeHelper
{
    /// <summary>
    /// Gets the local time in the local time zone.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="DateTime"/>.
    /// </returns>
    public static DateTime GetLocalTime()
    {
        return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local);
    }
}