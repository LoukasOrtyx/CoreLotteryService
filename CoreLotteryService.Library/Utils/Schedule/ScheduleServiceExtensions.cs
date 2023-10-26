using Microsoft.Extensions.DependencyInjection;

namespace CoreLotteryService.Library.Utils.Schedule;

/// <summary>
/// Utilitary class to hold extension methods of the <see cref="IServiceCollection"/>.
/// </summary>
public static class ScheduledServiceExtensions
{
    /// <summary>
    /// The extension method configures the ScheduleConfig, registers the ScheduleConfig as a 
    /// Singleton, and adds the T (a derived class of <see cref="CronJobService"/>) as a hosted
    /// service.
    /// </summary>
    /// <exception cref="ArgumentNullException"> when the Schedule Configuration or CronExpression
    /// are empty.
    /// </exception>
    public static IServiceCollection AddCronJob<T>
    (
        this IServiceCollection services,
        Action<IScheduleConfig<T>> options
    ) where T : CronJobService
    {
        if (options == null)
        {
            throw new ArgumentNullException
            (
                nameof(options), 
                @"Please provide Schedule Configurations."
            );
        }
        var config = new ScheduleConfig<T>();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException
            (
                nameof(ScheduleConfig<T>.CronExpression), 
                @"Empty Cron Expression is not allowed."
            );
        }
        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }
}   
