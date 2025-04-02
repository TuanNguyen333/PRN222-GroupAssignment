using System.Net.Http.Json;
using System.Text.Json;
using eStore.Models;
using Microsoft.Extensions.Logging;

namespace eStore.Services
{
    public class MemberService : IMemberService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MemberService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public MemberService(HttpClient httpClient, ILogger<MemberService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching members from {BaseUrl}", _httpClient.BaseAddress);
                var response = await _httpClient.GetAsync("api/members");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Members API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Members API Response Content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Member>>>(content, _jsonOptions);
                    
                    if (apiResponse?.Data?.Items == null)
                    {
                        _logger.LogWarning("API returned null data for members");
                        return new List<Member>();
                    }

                    _logger.LogInformation("Successfully retrieved {Count} members", apiResponse.Data.Items.Count);
                    return apiResponse.Data.Items.ToList();
                }
                
                _logger.LogError("Error fetching members. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                throw new HttpRequestException($"API returned status code: {response.StatusCode}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing members response");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching members");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching members");
                throw;
            }
        }

        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching member with ID {Id}", id);
                var response = await _httpClient.GetAsync($"api/members/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Member API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Member API Response Content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<Member>>(content, _jsonOptions);
                    if (apiResponse?.Data == null)
                    {
                        _logger.LogWarning("API returned null data for member ID {Id}", id);
                        return null;
                    }

                    _logger.LogInformation("Successfully retrieved member {Id}", id);
                    return apiResponse.Data;
                }
                
                _logger.LogError("Error fetching member. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching member with ID {Id}", id);
                return null;
            }
        }
    }
}
