# Introduction
## CoreLotteryService.Library
CoreLotteryService is a .NET library holding all relevant code to build Windows Services for the purpose of . Use 
it in a project by including it to the project's csproj file.


# Requirements
The necessary packages are listed bellow along with their minimal required version:
## C#:
- .NET 6.0.3;
- Deedle 2.5.0;
- Cronos 0.7.1;
- RestSharp 107.3.0;
- Newtonsoft.Json 13.0.1;
- Microsfoft.Playwright 1.22.0;
- Microsoft.Extensions.Hosting 6.0.1;
- Microsoft.Extensions.Hosting.WindowsServices 6.0.0.

# Example of usage
## Creating a new Windows Service:
In your project, run the command 
```git submodule add https://github.com/LoukasOrtyx/CoreLotteryService Submodules/CoreLotteryService```

In the visual studio, add in the solution folder a new folder called ```Submodules``` and inside it add this project as
an project, then in your target project dependencies, add this project as a project reference.

Then create a class that extends ```CoreLotteryService.Utils.CronJobService```, in this example 
"```DummyService```".

```cs
public class DummyService : CronJobService
{
    private readonly ILogger<DummyService> Logger;

    private readonly LotteryApiApiHandler Client;

    private readonly ServiceSettings Options;

    public DummyService
    (
        IScheduleConfig<DummyService> config,
        LotteryApiApiHandler LotteryApiApiHandler, 
        ILogger<DummyService> logger, 
        ServiceSettings options
    )
    : base(config.CronExpression, config.TimeZoneInfo)
    {
        Logger = logger;
        Client = LotteryApiApiHandler;
        Options = options;
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        /// Settings example:
        var testInfo = Options.Test;
        Logger.LogInformation(testInfo);
        ///

        /// Service Logic goes here.
    }
}
```

Also a appsettings.json is required containing the fields bellow, such as the example bellow:

```json
{
  "InstallationPath": "{the path you wish to install the service}",
  "Logging": {
    "LogLevel": {
      "Default": "{the log level}",
      "Microsoft.Hosting.Lifetime": "{the log level for Microsoft.Hosting.Lifetime}"
    }
  },
  "DummyService": {
    "DisplayName": "Test Dummy Service"
  }
```

Inside the DummyService field there should be its setting properties, which are found in 
[ServiceSettings.cs](Config/ServiceSettings.cs) (They are optional). If a new setting key is needed
it should be declared on ```ServiceSettings.cs``` previously.


Finally, in ```program.cs``` add: 
```cs
await Initializer.BuildWindowsService<DummyService>(args).RunAsync();
```
