namespace CoreLotteryService.Library.Helpers;

/// <summary>
/// An utility class that hold some useful methods of handling a string.
/// </summary
public static class StrHelper
{
    /// <summary>
    /// Truncates a  <see cref="string"/> up to a given length.
    /// </summary>
    /// <param name="value">The  <see cref="string"/> you want to truncate.</param>
    /// <param name="maxLength">The length up to which you want to to truncate</param>
    /// <returns>
    /// The resulting truncated <see cref="string"/>.
    /// </returns> 
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
    }
    /// <summary>
    /// Converts a <see cref="DateTime"/> to a <see cref="string"/> in the ISO 8061 format. 
    /// </summary>
    /// <param name="date">The date you wish to convert.</param>
    /// <returns>
    /// The resulting string.
    /// </returns> 
    public static string DateTime2IsoTime(this DateTime date)
    {
        return Truncate
        (
            date.ToString("o", System.Globalization.CultureInfo.InvariantCulture), 19
        ) + "Z";
    }
}


