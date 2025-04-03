using System.Net.Http.Json;
using System.Text.Json;
using eStore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using eStore.Hubs;
using Microsoft.JSInterop;
using eStore.Services.Common;

namespace eStore.Services
{
    public class MemberService : IMemberService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MemberService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHubContext<MemberHub> _hubContext;
        private readonly IJSRuntime _jsRuntime;
        private readonly StateContainer _stateContainer;

        public MemberService(
            HttpClient httpClient, 
            ILogger<MemberService> logger,
            IHubContext<MemberHub> hubContext,
            IJSRuntime jsRuntime,
            StateContainer stateContainer)
        {
            _httpClient = httpClient;
            _logger = logger;
            _hubContext = hubContext;
            _jsRuntime = jsRuntime;
            _stateContainer = stateContainer;
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

        public async Task<ApiResponse<Member>> CreateMemberAsync(Member member)
        {
            try
            {
                if (member == null)
                {
                    return new ApiResponse<Member>
                    {
                        Success = false,
                        Message = "Member cannot be null",
                        Errors = new[] { "Invalid member data" }
                    };
                }

                var response = await _httpClient.PostAsJsonAsync("api/members", member);
                var result = await HandleResponse<Member>(response);
                
                if (result.Success && result.Data != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMemberUpdate", "create", result.Data.MemberId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member");
                return new ApiResponse<Member>
                {
                    Success = false,
                    Message = "Error creating member",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Member>> UpdateMemberAsync(int id, Member member)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/members/{id}", member);
                var result = await HandleResponse<Member>(response);
                
                if (result.Success)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMemberUpdate", "update", id);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member");
                return new ApiResponse<Member>
                {
                    Success = false,
                    Message = "Error updating member",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteMemberAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/members/{id}");
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Delete response content: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Delete failed with status code: {StatusCode}", response.StatusCode);
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"API returned status code: {response.StatusCode}",
                        Data = false,
                        Errors = new[] { content }
                    };
                }

                // If response is empty or not valid JSON, consider delete successful
                if (string.IsNullOrWhiteSpace(content))
                {
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Member deleted successfully",
                        Data = true
                    };

                    // Send SignalR notification
                    try
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMemberUpdate", "delete", id);
                        _logger.LogInformation("SignalR notification sent for delete. MemberId: {Id}", id);
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. MemberId: {Id}", id);
                    }

                    return successResponse;
                }

                try 
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    };

                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, options);
                    _logger.LogInformation("Delete result: Success={Success}, Message={Message}, Data={Data}", 
                        result?.Success, result?.Message, result?.Data);

                    if (result == null)
                    {
                        // If can't deserialize but status code is success, consider delete successful
                        var successResponse = new ApiResponse<bool>
                        {
                            Success = true,
                            Message = "Member deleted successfully",
                            Data = true
                        };

                        // Send SignalR notification
                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveMemberUpdate", "delete", id);
                            _logger.LogInformation("SignalR notification sent for delete. MemberId: {Id}", id);
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. MemberId: {Id}", id);
                        }

                        return successResponse;
                    }

                    // If API returns success
                    if (result.Success)
                    {
                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveMemberUpdate", "delete", id);
                            _logger.LogInformation("SignalR notification sent for delete. MemberId: {Id}", id);
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. MemberId: {Id}", id);
                        }
                    }

                    return result;
                }
                catch (JsonException jsonEx)
                {
                    // If can't parse JSON but status code is success, consider delete successful
                    _logger.LogWarning(jsonEx, "Could not parse delete response as JSON, but status code indicates success");
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Member deleted successfully",
                        Data = true
                    };

                    // Send SignalR notification
                    try
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMemberUpdate", "delete", id);
                        _logger.LogInformation("SignalR notification sent for delete. MemberId: {Id}", id);
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. MemberId: {Id}", id);
                    }

                    return successResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete operation for MemberId: {Id}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting member",
                    Data = false,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Member>> GetCurrentUserAsync()
        {
            try
            {
                _logger.LogInformation("Fetching current user information");

                // Lấy token từ StateContainer
                var token = _stateContainer.AuthToken;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No authentication token found");
                    return new ApiResponse<Member>
                    {
                        Success = false,
                        Message = "Not authenticated"
                    };
                }

                // Thêm token vào header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/members/user");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Current user API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Current user API Response Content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<Member>>(content, _jsonOptions);
                    if (apiResponse == null)
                    {
                        _logger.LogWarning("API returned null response for current user");
                        return new ApiResponse<Member>
                        {
                            Success = false,
                            Message = "Invalid response format"
                        };
                    }

                    _logger.LogInformation("Successfully retrieved current user information");
                    return apiResponse;
                }
                
                _logger.LogError("Error fetching current user. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                return new ApiResponse<Member>
                {
                    Success = false,
                    Message = $"API returned status code: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user information");
                return new ApiResponse<Member>
                {
                    Success = false,
                    Message = "Error fetching user information",
                    Errors = new[] { ex.Message }
                };
            }
            finally
            {
                // Xóa token khỏi header sau khi hoàn thành
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("HandleResponse content: {Content}", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"API returned status code: {response.StatusCode}",
                    Errors = new[] { content }
                };
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, options);
                _logger.LogDebug("Deserialized result: Success={Success}, Message={Message}", 
                    result?.Success, result?.Message);

                if (result == null)
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Errors = new[] { "Invalid response format" }
                    };
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Error parsing response",
                    Errors = new[] { ex.Message }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing response: {Message}", ex.Message);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Error processing response",
                    Errors = new[] { ex.Message }
                };
            }
        }
    }
}
