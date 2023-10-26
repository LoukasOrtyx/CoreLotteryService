using RestSharp;
using Newtonsoft.Json;
using RestSharp.Serializers;

namespace CoreLotteryService.Library.Utils.Api;
/// <summary>
/// Base class with most API handling capabilities implemented.
/// </summary>
public abstract class BaseApiHandler : RestClient
{
    /// <summary>
    /// Represents the access token.
    /// </summary>
    protected string Token { get; set; }
    /// <summary>
    /// Injects the API url into an instance of <see cref="RestClientOptions"/>.
    /// </summary>
    /// <remarks>
    /// <param name="url">Web API url.</param>
    /// Currently it bypass certifications, which must not be the case in production.
    /// </remarks>
    /// <returns>
    /// An instance of <see cref="RestClientOptions"/>.
    /// </returns> 
    protected static RestClientOptions GetClientOptions(string url)
    {
            return new RestClientOptions
            {
                BaseUrl = new Uri(url),
                RemoteCertificateValidationCallback = 
                    (sender, certificate, chain, sslPolicyErrors) => true,
                Timeout = 60000
            };
    }
    /// <summary>
    /// Constructor. Injects options.
    /// </summary>
    /// <param name="url">Web API url.</param>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns> 
    protected BaseApiHandler(string url) : base(GetClientOptions(url)) {}
    /// <summary>
    /// Get the login parameters used to obtain <see cref="Token"/>.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="RestRequest"/> with all parameters set for login operation.
    /// </returns> 
    protected abstract RestRequest GetTokenRequest();
    /// <summary>
    /// Send the login request to the LotteryApi API and stores the access token in 
    /// <see cref="Token"/>.
    /// </summary>
    /// <returns>
    /// <see cref="void"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the response to the token request is empty or null. 
    /// </exception>
    protected virtual async Task SetToken()
    {
        RestRequest tokenRequest = GetTokenRequest();
        RestResponse tokenResponse = await this.GetRestResponse(tokenRequest);
        string responseContent = tokenResponse.Content ?? "";
        var responseDict = JsonConvert.
            DeserializeObject<Dictionary<string, object>>(responseContent);
        if (responseDict == null)
        {
            throw new ArgumentNullException();
        }
        string accessToken = (string) responseDict["access_token"];
        this.Token = $"Bearer {accessToken}";
    }
    /// <summary>
    /// Set the headers for default CRUD requests to the LotteryApi API.
    /// </summary>
    /// <param name="request">A given request.</param>
    /// <returns>
    /// The resulting request containing the request paramaters.
    /// </returns> 
    /// <exception cref="ArgumentNullException">
    /// Thrown when something really weird happened and the token for some reason is not found 
    /// anywhere. It can happen if you add breaklines to the project.
    /// </exception>
    protected virtual async Task<RestRequest> SetRequestHeaders(RestRequest request)
    {
        if (Token == null)
        {
            await SetToken();
        }
        request.AddHeader
        (
            "Authorization", 
            this.Token ?? 
            throw new ArgumentNullException("Something went wrong and the token is missing")
        );
        request.AddHeader("Accept-Encoding", "gzip, deflate");
        return request;
    }
    /// <summary>
    /// Send a request and handles the response accordingly by checking if the status code indicates
    ///  a successful request, throwing an exception otherwise.
    /// </summary>
    /// <param name="response">A given request.</param>
    /// <returns>
    /// The resulting <see cref="RestResponse"/> instance.
    /// </returns> 
    /// <exception cref="HttpRequestException">
    /// Thrown when the request is unsuccessful.
    /// </exception>

    protected virtual async Task<RestResponse> GetRestResponse(RestRequest request)
    {
        RestResponse response = await this.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            throw new HttpRequestException(response.ErrorMessage, null, response.StatusCode);
        }
        return response;
    }
    /// <summary>
    /// Execute the request and returns the response.
    /// </summary>
    /// <param name="request">A given request.</param>
    /// <returns>
    /// The resulting <see cref="RestResponse"/> instance.
    /// </returns> 
    public async Task<RestResponse> ExecuteRequest(RestRequest request)
    {
        request = await SetRequestHeaders(request);
        return await GetRestResponse(request);
    }
    /// <summary>
    /// Execute the request and returns the response. Use this overload when the request doesn't
    /// require a payload.
    /// </summary>
    /// <param name="route">LotteryApi API route.</param>
    /// <param name="method">The request operation type, e.g.: POST, GET, etc.</param>
    /// <returns>
    /// The resulting <see cref="RestResponse"/> instance.
    /// </returns> 
    public async Task<RestResponse> ExecuteRequest(string route, Method method)
    {
        RestRequest request = new RestRequest(route, method);
        request = await SetRequestHeaders(request);
        return await GetRestResponse(request);
    }
    /// <summary>
    /// Execute the request and returns the response.
    /// </summary>
    /// <param name="route">LotteryApi API route.</param>
    /// <param name="method">The request operation type, e.g.: POST, GET, etc.</param>
    /// <param name="payload">any object.</param>
    /// <returns>
    /// The resulting <see cref="RestResponse"/> instance.
    /// </returns> 
    public virtual async Task<RestResponse> ExecuteRequest
    (
        string route, 
        Method method, 
        object payload
    )
    {
        RestRequest request = new RestRequest(route, method);
        request = await SetRequestHeaders(request);
        request.AddJsonBody(payload);
        return await GetRestResponse(request);
    }
    /// <summary>
    /// Execute the request and returns the response.
    /// </summary>
    /// <remarks>
    /// This overload is meant to simplify HTTP operations by offering a easier way to add requests 
    /// to body without having to worry about types and erroneous convertions made internally by 
    /// RestSharp.
    /// </remarks>
    /// <param name="route">LotteryApi API route.</param>
    /// <param name="method">The request operation type, e.g.: POST, GET, etc.</param>
    /// <param name="payload">A raw JSON string.</param>
    /// <returns>
    /// The resulting <see cref="RestResponse"/> instance.
    /// </returns> 
    public virtual async Task<RestResponse> ExecuteRequest
    (
        string route, 
        Method method, 
        string payload
    )
    {
        RestRequest request = new RestRequest(route, method);
        request = await SetRequestHeaders(request);
        request.AddStringBody(payload, ContentType.Json);
        return await GetRestResponse(request);
    }
}