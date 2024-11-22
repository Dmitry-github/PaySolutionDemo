namespace ConsoleApp
{
    using Polly;
    using Polly.Retry;
    using System.Net.Http.Headers;

    public interface INetworkRequestHandler
    {
        public Task<HttpResponseMessage> MakeGetRequestAsync(string requestUri);
        public Task<HttpResponseMessage> MakePostRequestAsync(StringContent content);
    }

    public class NetworkRequestHandler: INetworkRequestHandler
    {
        //TODO: to secrets
        private const string KeyId = "47e8fde35b164e888a57b6ff27ec020f";
        
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly string? _host;
        private readonly string? _requestUri;

        public NetworkRequestHandler(string? host, string? reqestUri)
        {
            _host = host;
            _requestUri = reqestUri;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_host);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("kid", KeyId);

            //retry on HttpRequestException, 3 retries with exponential backoff
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Request failed with {outcome.Exception?.Message}.Waiting {timespan} before next retry.Retry attempt {retryAttempt}.");

                    });
        }

        public async Task<HttpResponseMessage> MakeGetRequestAsync(string requestUri)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                Console.WriteLine($"Making request to {requestUri}");
                var response = await _httpClient.GetAsync(requestUri);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request failed with status code: { response.StatusCode}");
                    throw new HttpRequestException($"Request to {requestUri} failed with status code { response.StatusCode }");
                }
                return response;
            });
        }

        public async Task<HttpResponseMessage> MakePostRequestAsync(StringContent content)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                //TODO: Log
                Console.WriteLine($"Making request to {_host}{_requestUri}");

                var response = await _httpClient.PostAsync(_requestUri, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    throw new HttpRequestException($"Request to {_host} failed with status code {response.StatusCode}");
                }
                return response;
            });
        }
    }
}
