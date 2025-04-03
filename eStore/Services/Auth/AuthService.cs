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
        private readonly StateContainer _stateContainer;

        public AuthService(IHttpClientFactory httpClientFactory, StateContainer stateContainer)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _stateContainer = stateContainer;
        }

        public async Task<ApiResponse<AuthenticationResponse>> Login(LoginDto loginDto)
        {
            Console.WriteLine("Attempting to login...");
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Login request successful");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthenticationResponse>>();
                if (result?.Success == true && result.Data != null)
                {
                    Console.WriteLine($"Login successful, token length: {result.Data.Token?.Length ?? 0}");
                    _stateContainer.SetAuthData(result.Data.Token, result.Data.ExpirationTime);
                    Console.WriteLine("Token stored in StateContainer");
                    return result;
                }
                
                Console.WriteLine($"Login response not successful or data is null. Success: {result?.Success}, HasData: {result?.Data != null}");
                return result ?? ApiResponse<AuthenticationResponse>.ErrorResponse("Invalid response format");
            }
            
            Console.WriteLine($"Login failed with status code: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error content: {errorContent}");
            return ApiResponse<AuthenticationResponse>.ErrorResponse("Login failed");
        }

        public async Task<bool> Logout()
        {
            try
            {
                Console.WriteLine("Logging out...");
                
                // Gọi API logout
                var response = await _httpClient.GetAsync("api/auth/logout");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Logout API response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    // Xóa dữ liệu xác thực ở client side
                    _stateContainer.ClearAuthData();
                    Console.WriteLine("Auth data cleared from StateContainer");
                    return true;
                }
                
                Console.WriteLine($"Logout failed with status code: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                // Vẫn xóa dữ liệu xác thực ở client side trong trường hợp lỗi
                _stateContainer.ClearAuthData();
                return false;
            }
        }

        public Task<bool> IsAuthenticated()
        {
            var isAuth = _stateContainer.IsAuthenticated;
            Console.WriteLine($"Checking authentication status: {isAuth}");
            return Task.FromResult(isAuth);
        }
    }
}
