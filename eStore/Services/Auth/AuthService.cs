using System.Net.Http.Json;
using System.Text.Json;
using BusinessObjects.Dto.Auth;
using BusinessObjects.Base;
using eStore.Services.Common;

namespace eStore.Services.Auth
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorageService _localStorage;

        public AuthService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<AuthenticationResponse>> Login(LoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthenticationResponse>>();
                if (result?.Success == true && result.Data != null)
                {
                    await _localStorage.SetItemAsync("authToken", result.Data.Token);
                    await _localStorage.SetItemAsync("expirationTime", result.Data.ExpirationTime.ToString());
                }
                return result;
            }
            
            return ApiResponse<AuthenticationResponse>.ErrorResponse("Login failed");
        }

        public async Task Logout()
        {
            var response = await _httpClient.GetAsync("api/auth/logout");
            if (response.IsSuccessStatusCode)
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("expirationTime");
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(token);
        }
    }
}
