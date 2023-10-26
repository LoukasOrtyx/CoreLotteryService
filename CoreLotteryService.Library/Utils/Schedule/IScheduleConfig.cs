namespace CoreLotteryService.Library.Utils.Schedule;

/// <summary>
/// Holds the <see cref="CronExpression"/> and <see cref="TimeZoneInfo"/>, both used to configure a
/// <see cref="CronJobService"/> properly.
/// </summary>
public interface IScheduleConfig<T>
{
    string CronExpression { get; set; }
    TimeZoneInfo TimeZoneInfo { get; set; }
}
/// <summary>
/// Implementation of the custom interface <see cref="IScheduleConfig"/>
/// </summary>
public class ScheduleConfig<T> : IScheduleConfig<T>
{
    public string CronExpression { get; set; }
    public TimeZoneInfo TimeZoneInfo { get; set; }
}