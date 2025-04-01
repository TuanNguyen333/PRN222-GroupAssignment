// eStore/Services/AuthenticationHeaderHandler.cs
using System.Net.Http.Headers;

namespace eStore.Services
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
            // Use StateContainer instead of localStorage directly
            if (_stateContainer.IsAuthenticated)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _stateContainer.AuthToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
