using Cronos;
using RestSharp;
using CoreLotteryService.Library.Config;
using CoreLotteryService.Library.Helpers;
using CoreLotteryService.Library.Utils.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace CoreLotteryService.Library.Utils;

/// <summary>
/// Abstract class to systematically configure and register schedulable Hosted Services using cron
/// expressions.
/// </summary> 
public abstract class CronJobService : IHostedService, IDisposable
{
    /// <summary>
    /// Enumerables to inform service status (whether the work carried out was successful or not).
    /// </summary>
    protected enum ServiceWorkStatus : int
    {
        Failure = 0,
        Success = 1
    }
    /// <summary>
    /// Logger from dependency injection.
    /// </summary>
    protected readonly ILogger<CronJobService> Logger;
    /// <summary>
    /// Configuration singleton class from depedency injection.
    /// </summary>
    protected readonly ServiceSettings Options;
    /// <summary>
    /// CronTimer to measure precisely the "business hours" of the instance of 
    /// <see cref="CronJobService"/>.
    /// </summary> 
    private System.Timers.Timer CronTimer;
    /// <summary>
    /// Lottery Web API Handler singleton from dependency injection.
    /// </summary>
    protected readonly LotteryApiHandler LotteryClient;
    /// <summary>
    /// Text representing the cron job expression.
    /// </summary> 
    /// <summary>
    /// Psr Core API Handler singleton from dependency injection.
    /// </summary>
    private readonly CronExpression Expression;
    /// <summary>
    /// Dictates the service time zone.
    /// </summary> 
    private readonly TimeZoneInfo CronTimeZone;
    /// <summary>
    /// <see cref="CronJobService"/> constructor.
    /// </summary>
    /// <param name="cronExpression"><see cref="Expression"/>.</param>
    /// <param name="timeZoneInfo"><see cref="CronTimeZone"/>.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>  
    protected CronJobService
    (
        string cronExpression,
        LotteryApiHandler LotteryApiHandler,
        TimeZoneInfo timeZoneInfo, 
        ServiceSettings options,
        ILogger<CronJobService> logger
    )
    {
        Logger = logger;
        Options = options;
        LotteryClient = LotteryApiHandler;
        Expression = CronExpression.Parse(cronExpression);
        CronTimeZone = timeZoneInfo;
    }
    /// <summary>
    /// Uses the <see cref="LotteryClient"/> to send a request to inform whether the 
    /// service was successful in its task (<see cref="DoWork"/>). 
    /// </summary>
    /// <param name="svcStatus"><see cref="ServiceWorkStatus"/>.</param>
    /// <returns>
    /// <see cref="Task"/>.
    /// </returns>  
    protected async Task SendStatusToApi(ServiceWorkStatus svcStatus)
    {
        var req = new Dictionary<string, object>
        {
            {"ServiceName", Options.DisplayName},
            {"Status", svcStatus},
            {"Date", DateTimeHelper.GetLocalTime()},
        };
        string rawStrRequest = Newtonsoft.Json.JsonConvert.SerializeObject(req);
        try
        {
            await LotteryClient.ExecuteRequest("api/LotterySvcWorkStatus", Method.Post, rawStrRequest);
        } catch (HttpRequestException httpError) 
        {
            Logger.LogError(httpError.Message);
        }
    }
    /// <summary>
    /// Writes a json file to determine current active status and last time it changed.
    /// </summary>
    /// <param name="activeStatus">Whether it Started or Stopped.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>  
    protected void WriteCurrentStatus(string activeStatus)
    {
        var jsonDict = new Dictionary<string, object>
        {
            {"Status", activeStatus},
            {"Date", DateTimeHelper.GetLocalTime()},
        };
        string rawStrJson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonDict);
        string jsonPath = Path.Combine(AppContext.BaseDirectory, "currentStatus.json");
        using (StreamWriter streamWriter = new StreamWriter(jsonPath, false))
        {
            streamWriter.WriteLine(rawStrJson);
            streamWriter.Close();
        }
    }
    /// <summary>
    /// Starts the schedule of the job and logs its current status.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>
    /// <see cref="Task"/>.
    /// </returns>  
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{Options.DisplayName} has started successfully");
        WriteCurrentStatus("Started");
        await ScheduleJob(cancellationToken);
    }
    /// <summary>
    /// Computes the next occurrence based on the cron expression, then it starts a timer with a 
    /// certain delay. When the time is due, the timer will raise an Elapsed event, which stops the 
    /// timer, executes the background task <see cref="DoWork"/>, and schedules the job recursively.
    /// </summary>
    /// <remarks>
    /// The status of the work carried out is sent to the Lottery Web API for monitoring purposes,
    /// informing if it was a success or not. Furthermore, any catched exception will result in a 
    /// service stop, which will also be logged and sent to the API.
    /// </remarks>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>
    /// <see cref="Task"/>.
    /// </returns>
    protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = Expression.GetNextOccurrence(DateTimeOffset.Now, CronTimeZone);
        if (next.HasValue)
        {
            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJob(cancellationToken);
            }
            CronTimer = new System.Timers.Timer(delay.TotalMilliseconds);
            CronTimer.Elapsed += async (sender, args) =>
            {
                CronTimer.Dispose();
                CronTimer = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    // Assumes it failed until it runs DoWork method.
                    ServiceWorkStatus svcStatus = ServiceWorkStatus.Failure;
                    try
                    {
                        await DoWork(cancellationToken);
                        svcStatus = ServiceWorkStatus.Success;
                    } catch (Exception e)
                    {
                        WriteCurrentStatus("Stopped");
                        Logger.LogCritical(e.ToString());
                        throw;
                    } finally
                    {
                        await SendStatusToApi(svcStatus);
                    }
                }
                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);
                }
            };
            CronTimer.Start();
        }
        await Task.CompletedTask;
    }
    /// <summary>
    /// Abstract method where the windows service task is implemented.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>
    /// <see cref="Task"/>.
    /// </returns>  
    public abstract Task DoWork(CancellationToken cancellationToken);
    /// <summary>
    /// Stops the task and logs its current status
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>
    /// <see cref="Task"/>.
    /// </returns>  
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        CronTimer?.Stop();
        WriteCurrentStatus("Stopped");
        Logger.LogInformation($"{Options.DisplayName} has been stopped");
        await Task.CompletedTask;
    }
    /// <summary>
    /// Disposes properly of the task by releasing all resources from the <see cref="CronTimer"/>.
    /// </summary>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>  
    public virtual void Dispose()
    {
        CronTimer?.Dispose();
    }
}