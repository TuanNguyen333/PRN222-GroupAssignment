// eStore/Services/AuthenticationHeaderHandler.cs
using eStore.Services.Common;
using System.Net.Http.Headers;

namespace eStore.Services.Auth
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly StateContainer _stateContainer;

        public AuthenticationHeaderHandler(StateContainer stateContainer)
        {
            _stateContainer = stateContainer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _stateContainer.AuthToken;
            Console.WriteLine($"Processing request to: {request.RequestUri}");
            
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"Adding auth token to request. Token length: {token.Length}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // Log all headers for debugging
                foreach (var header in request.Headers)
                {
                    Console.WriteLine($"Header: {header.Key} = {string.Join(", ", header.Value)}");
                }
            }
            else
            {
                Console.WriteLine("No auth token found in StateContainer");
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            Console.WriteLine($"Response status code: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Received unauthorized response from API");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");
            }

            return response;
        }
    }
}
