using RestSharp;

namespace CoreLotteryService.Library.Utils.Api;
/// <summary>
/// Handles all HTTP interactions with the Lottery Web API.
/// </summary>
public class LotteryApiHandler : BaseApiHandler
{
    /// <summary>
    /// Constructor. Injects options.
    /// </summary>
    /// <param name="url">Lottery web api url.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns> 
    public LotteryApiHandler(string url) : base(url: url) {}
    /// <summary>
    /// Override not implemented yet because no token is needed for the time being.
    /// </summary>
    /// <inheritdoc/>
    protected override RestRequest GetTokenRequest()
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    protected override Task<RestRequest> SetRequestHeaders(RestRequest request)
    {
        request.AddHeader("Accept-Encoding", "gzip, deflate");
        return Task.Run(() => request);
    }
}