using CoreLotteryService.Library.Utils.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLotteryService.Library.Utils.Logger;

public static class ApplicationLoggerFactoryExtensions
{
    public static ILoggingBuilder CustomLogger(this ILoggingBuilder builder, string serviceName)
    {
        builder.Services.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => 
            options.MinLevel = LogLevel.Error);
        builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>
        (
            p => new LoggerProvider(p.GetService<LotteryApiHandler>(), serviceName)
        );
        return builder;
    }
}