using CoreLotteryService.Library.Utils.Api;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CoreLotteryService.Library.Utils.Logger;

/// <summary>
/// Custom Logger Provider to be injected into an of <see cref="CronJobService"/>.
/// </summary> 
public class LoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Lottery Web API Handler singleton from dependency injection.
    /// </summary>
    readonly LotteryApiHandler Client;
    /// <summary>
    /// Name of the service logging.
    /// </summary>
    readonly string ServiceName;
    /// <summary>
    /// A dictionary of loggers.
    /// </summary> 
    readonly ConcurrentDictionary<string, Logger> loggers = 
        new ConcurrentDictionary<string, Logger>();
    /// <summary>
    /// <see cref="LoggerProvider"/> constructor.
    /// </summary>
    /// <param name="client"><see cref="client"/>.</param>
    /// <param name="serviceName"><see cref="serviceName"/>.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>  
    public LoggerProvider(LotteryApiHandler client, string serviceName)
    {
        Client = client;
        ServiceName = serviceName;
    }
    /// <summary>
    /// Creates logger.
    /// </summary>
    /// <param name="category">The category of the new logger.</param>
    /// <returns>
    /// An instance of <see cref="ILogger"/>.
    /// </returns>  
    public ILogger CreateLogger(string category)
    {
        return loggers.GetOrAdd(category, name => new Logger(name, Client, ServiceName));
    }

    public void Dispose() 
    {
    }
}

