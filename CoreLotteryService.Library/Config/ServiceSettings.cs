namespace CoreLotteryService.Library.Config;

/// <summary>
/// POCO class to store the service settings.
/// </summary>
/// <remarks>
/// Currently it stores specific parameters service-wise all in the same class, which is not ideal.
/// For example, a service might not need some of the parameters, thus it will be null because the
/// appsettings won't have it linked to the service.
/// </remarks>
public class ServiceSettings
{
    public string CceeUrl { get; set; }
    public string CronJob { get; set; }
    public string StudyName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int Scenarios {get; set; }
    public string EpmUser { get; set; }
    public string EpmPassword { get; set; }
    public string EpmAuth { get; set; }
    public string EpmWeb { get; set; }
}