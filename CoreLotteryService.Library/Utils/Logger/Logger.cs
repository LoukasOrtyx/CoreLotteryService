using RestSharp;
using CoreLotteryService.Library.Helpers;
using CoreLotteryService.Library.Utils.Api;
using Microsoft.Extensions.Logging;

namespace CoreLotteryService.Library.Utils.Logger;

/// <summary>
/// Custom Logger to allow writing to file.
/// </summary>
public class Logger : ILogger
{
    /// <summary>
    /// Name of the logger.
    /// </summary>
    readonly string Name;
    /// <summary>
    /// Name of the service logging.
    /// </summary>
    readonly string ServiceName;
    /// <summary>
    /// Lottery Web API Handler singleton from dependency injection.
    /// </summary>
    readonly LotteryApiHandler Client;
    /// <summary>
    /// <see cref="Logger"/> constructor.
    /// </summary>
    /// <param name="name"><see cref="Name"/>.</param>
    /// <param name="LotteryApiHandler"><see cref="LotteryApiHandler"/>.</param>
    /// <param name="serviceName"><see cref="ServiceName"/>.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>
    public Logger(string name, LotteryApiHandler LotteryApiHandler, string serviceName)
    {
        Name = name;
        ServiceName = serviceName;
        Client = LotteryApiHandler;
    }
    /// <summary>
    /// Starts scope.
    /// </summary>
    /// <remarks>
    /// Didn't know how to use it, so I returned null instead.
    /// </remarks>
    /// <param name="value">An instance of <see cref="TState"/>.</param>
    /// <returns>
    /// An instance of <see cref="IDisposable"/> with scope.
    /// </returns>
    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }
    /// <summary>
    /// Checks if the log level is allowed.
    /// </summary>
    /// <param name="logLevel">The desired Log level.</param>
    /// <returns>
    /// <see cref="true"/> if it is and <see cref="false"/> otherwise.
    /// </returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Writes message to file inside base directory.
    /// </summary>
    /// <param name="message">The custom text.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>
    private void WriteToFile(string message)
    {
        string logPath = Path.Combine(AppContext.BaseDirectory, "log.txt");
        using (StreamWriter streamWriter = new StreamWriter(logPath, true))
        {
            streamWriter.WriteLine(message);
            streamWriter.Close();
        }
    }
    /// <summary>
    /// Creates a Log entity and send it to Lottery Web API.
    /// </summary>
    /// <remarks>
    /// If the request to the Lottery Web API fails, it logs the attempt in the log.txt file as error.
    /// </remarks>
    /// <param name="logLevel">The level of the log.</param>
    /// <param name="time">When event happened.</param>
    /// <param name="message">Message related to the event being logged.</param>
    /// <returns>
    /// <see cref="Task"/>.
    /// </returns>
    private async Task SendToApi(LogLevel logLevel, DateTime time, string message)
    {
        var log = new Dictionary<string, object>
            {
                {"ServiceName", ServiceName},
                {"Date", time},
                {"LogLevel", logLevel.ToString()},
                {"Message", message}
            };
        string rawStrRequest = Newtonsoft.Json.JsonConvert.SerializeObject(log);
        try
        {
            RestResponse response = await Client.ExecuteRequest
            (
                "api/Logs", 
                Method.Post, 
                rawStrRequest
            );
        } catch (HttpRequestException httpError)
        {
            WriteToFile
            (
                $"{LogLevel.Warning}: Couldn't send log to API -> Status Code: " +
                $"({httpError.StatusCode})  Error message: ({httpError.Message})"
            );
        }
    }
    /// <summary>
    /// Writes to console and to a file a logging message.
    /// </summary>
    /// <param name="logLevel">The level of the log.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="state">The <see cref="TState"/> instance.</param>
    /// <param name="exception">The throwed exception or custom text.</param>
    /// <param name="formatter">Formatter of an exception.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        DateTime now = DateTimeHelper.GetLocalTime();
        string message =
            $"({now.ToString()}) {logLevel.ToString()}: {eventId.Id} - " +
            $"{formatter(state, exception)}";
        WriteToFile(message);
        if (Client != null)
        {
            Task.WaitAll(Task.Run(async () => await SendToApi(
                logLevel,
                now,
                formatter(state, exception))));
        }
    }
}

