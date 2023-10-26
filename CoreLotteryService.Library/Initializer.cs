using CoreLotteryService.Library.Config;
using CoreLotteryService.Library.Utils;
using CoreLotteryService.Library.Utils.Api;
using CoreLotteryService.Library.Utils.Logger;
using CoreLotteryService.Library.Utils.Schedule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreLotteryService.Library;

/// <summary>
/// Utilitary class to start building a Windows Service with injected logging using the custom 
/// <see cref="Logger"/> and LotteryApi communication capabilities with the 
/// <see cref="LotteryApiApiHandler"/> class.
/// </summary> 
public static class Initializer {
	/// <summary>
	/// Logger just to log errors if any on initialization.
	/// </summary>
	private static Logger InitLogger = new Logger("CoreLotteryService.Library", null, "");
	/// <summary>
	/// Creates a new WindowsService with some predefined configurations from <see cref="CoreLotteryService.Library"/>
	/// of some class derived of <see cref="CronJobService"/>.
	/// </summary>
	/// <remarks>
	/// Adds a logger and a LotteryApiAPI class to the <see cref="IServiceCollection"/>.
	/// </remarks>
	/// <returns>
	/// An instance of <see cref="IHostBuilder"/>.
	/// </returns>
	public static IHost BuildWindowsService<T>
	(
		string[] args
	) where T : CronJobService {
		IHost host;
		string svcName = typeof(T).Name;
		try {
			host = Host.CreateDefaultBuilder(args)
				.UseWindowsService()
				.ConfigureAppConfiguration((hostingContent, config) => {
					// Includes testing appsettings for unit test purposes;
					config.AddJsonFile
					(
						Path.Combine
						(
							AppContext.BaseDirectory,
							$"appsettings.Test.json"
						),
						optional: true
					);
					// Default appsettings for Development and Production environments.
					config.AddJsonFile
					(
						Path.Combine(AppContext.BaseDirectory, $"appsettings.json"), optional: true
					);
				})
				.ConfigureServices((hostContent, services) => {
					IConfiguration config = hostContent.Configuration;
					string LotteryApiUrl = config.GetSection("LotteryApiUrl").Get<string>();
					services.AddSingleton<LotteryApiHandler>(new LotteryApiHandler(LotteryApiUrl));
					ServiceSettings jobConfig = config.GetSection(svcName)
													.Get<ServiceSettings>();
					services.AddCronJob<T>(c => {
						c.TimeZoneInfo = TimeZoneInfo.Local;
						c.CronExpression = jobConfig.CronJob;
					});
					services.AddSingleton(jobConfig);
					services.AddLogging(configure => configure.CustomLogger(jobConfig.DisplayName));
				}).Build();
		}
		catch (Exception e) {
			InitLogger.LogCritical($"{svcName} failed to start");
			InitLogger.LogCritical(e.ToString());
			throw;
		}
		return host;
	}
}