using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using eStore.Hubs;
using eStore.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace eStore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IHubContext<CategoryHub> _hubContext;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            HttpClient httpClient,
            IHubContext<CategoryHub> hubContext,
            ILogger<CategoryService> logger)
        {
            _httpClient = httpClient;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResponse<Category>>> GetAllCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add($"search={search}");
                }

                var url = $"api/categories?{string.Join("&", queryParams)}";
                _logger.LogInformation("Requesting URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Response content: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned error status code: {StatusCode}", response.StatusCode);
                    return new ApiResponse<PagedResponse<Category>>
                    {
                        Success = false,
                        Message = $"API returned error status code: {response.StatusCode}",
                        Data = null,
                        Errors = new[] { content }
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Category>>>(content, options);

                if (result == null)
                {
                    _logger.LogWarning("Failed to deserialize response");
                    return new ApiResponse<PagedResponse<Category>>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Data = null,
                        Errors = new[] { "Invalid response format" }
                    };
                }

                _logger.LogInformation("Success: {Success}, Message: {Message}", result.Success, result.Message);
                if (result.Data != null)
                {
                    _logger.LogInformation("Total Items: {TotalItems}, Total Pages: {TotalPages}, Current Page: {CurrentPage}", 
                        result.Data.TotalItems, result.Data.TotalPages, result.Data.PageNumber);
                    if (result.Data.Items != null)
                    {
                        _logger.LogInformation("Items count: {Count}", result.Data.Items.Count);
                    }
                }

                // If no categories were found, return a successful response with empty list
                if (result.Data?.Items == null || !result.Data.Items.Any())
                {
                    result.Data = new PagedResponse<Category>
                    {
                        Items = new List<Category>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalItems = 0,
                        TotalPages = 1
                    };
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HttpRequestException: {Message}", ex.Message);
                return new ApiResponse<PagedResponse<Category>>
                {
                    Success = false,
                    Message = "Network error while fetching categories",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories: {Message}", ex.Message);
                return new ApiResponse<PagedResponse<Category>>
                {
                    Success = false,
                    Message = "Error retrieving categories",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Category>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/categories/{id}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetCategoryByIdAsync response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<Category>
                    {
                        Success = false,
                        Message = $"API returned status code: {response.StatusCode}",
                        Data = null,
                        Errors = new[] { content }
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<Category>>(content, options);
                    Console.WriteLine($"Deserialized result: Success={result?.Success}, Message={result?.Message}, Data={result?.Data != null}");

                    if (result == null)
                    {
                        _logger.LogWarning("Failed to deserialize category response for ID: {Id}", id);
                        return new ApiResponse<Category>
                        {
                            Success = false,
                            Message = "Failed to deserialize response",
                            Data = null,
                            Errors = new[] { "Invalid response format" }
                        };
                    }

                    if (!result.Success || result.Data == null)
                    {
                        _logger.LogWarning("Category not found or API returned unsuccessful response for ID: {Id}", id);
                        return new ApiResponse<Category>
                        {
                            Success = false,
                            Message = result.Message ?? "Category not found",
                            Data = null,
                            Errors = result.Errors ?? new[] { $"No category exists with ID {id}" }
                        };
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error for category ID: {Id}. Content: {Content}", id, content);
                    return new ApiResponse<Category>
                    {
                        Success = false,
                        Message = "Error parsing category data",
                        Data = null,
                        Errors = new[] { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {Id}", id);
                return new ApiResponse<Category>
                {
                    Success = false,
                    Message = "Error retrieving category",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Category>> CreateCategoryAsync(Category category)
        {
            try
            {
                if (category == null)
                {
                    return new ApiResponse<Category>
                    {
                        Success = false,
                        Message = "Category cannot be null",
                        Errors = new[] { "Invalid category data" }
                    };
                }

                _logger.LogInformation("Creating category: {CategoryName}", category.CategoryName);
                var response = await _httpClient.PostAsJsonAsync("api/categories", category);
                var result = await HandleResponse<Category>(response);
                
                if (result.Success && result.Data != null)
                {
                    try
                    {
                        _logger.LogWarning("⭐ Sending CREATE notification via SignalR for CategoryId: {CategoryId} ⭐", result.Data.CategoryId);
                        Console.WriteLine($"⭐ Sending CREATE notification via SignalR for CategoryId: {result.Data.CategoryId} ⭐");
                        
                        await _hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", "create", result.Data.CategoryId);
                        
                        _logger.LogWarning("✅ SignalR create notification sent successfully for CategoryId: {CategoryId}", result.Data.CategoryId);
                        Console.WriteLine($"✅ SignalR create notification sent successfully for CategoryId: {result.Data.CategoryId}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "⚠️ Failed to send SignalR notification for category create. CategoryId: {Id}", result.Data.CategoryId);
                        Console.WriteLine($"⚠️ Failed to send SignalR notification for category create. CategoryId: {result.Data.CategoryId}, Error: {signalREx.Message}");
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return new ApiResponse<Category>
                {
                    Success = false,
                    Message = "Error creating category",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Category>> UpdateCategoryAsync(int id, Category category)
        {
            try
            {
                _logger.LogInformation("Updating category: {CategoryId}", id);
                var response = await _httpClient.PutAsJsonAsync($"api/categories/{id}", category);
                var result = await HandleResponse<Category>(response);
                
                if (result.Success)
                {
                    try
                    {
                        _logger.LogWarning("⭐ Sending UPDATE notification via SignalR for CategoryId: {CategoryId} ⭐", id);
                        Console.WriteLine($"⭐ Sending UPDATE notification via SignalR for CategoryId: {id} ⭐");
                        
                        await _hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", "update", id);
                        
                        _logger.LogWarning("✅ SignalR update notification sent successfully for CategoryId: {CategoryId}", id);
                        Console.WriteLine($"✅ SignalR update notification sent successfully for CategoryId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "⚠️ Failed to send SignalR notification for category update. CategoryId: {Id}", id);
                        Console.WriteLine($"⚠️ Failed to send SignalR notification for category update. CategoryId: {id}, Error: {signalREx.Message}");
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return new ApiResponse<Category>
                {
                    Success = false,
                    Message = "Error updating category",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting category: {CategoryId}", id);
                var response = await _httpClient.DeleteAsync($"api/categories/{id}");
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
                        Message = "Category deleted successfully",
                        Data = true
                    };

                    // Send SignalR notification
                    try
                    {
                        _logger.LogWarning("⭐ Sending DELETE notification via SignalR for CategoryId: {CategoryId} ⭐", id);
                        Console.WriteLine($"⭐ Sending DELETE notification via SignalR for CategoryId: {id} ⭐");
                        
                        await _hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", "delete", id);
                        
                        _logger.LogWarning("✅ SignalR delete notification sent successfully for CategoryId: {CategoryId}", id);
                        Console.WriteLine($"✅ SignalR delete notification sent successfully for CategoryId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "⚠️ Failed to send SignalR notification for delete. CategoryId: {Id}", id);
                        Console.WriteLine($"⚠️ Failed to send SignalR notification for delete. CategoryId: {id}, Error: {signalREx.Message}");
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
                            Message = "Category deleted successfully",
                            Data = true
                        };

                        // Send SignalR notification
                        try
                        {
                            _logger.LogWarning("⭐ Sending DELETE notification via SignalR for CategoryId: {CategoryId} ⭐", id);
                            Console.WriteLine($"⭐ Sending DELETE notification via SignalR for CategoryId: {id} ⭐");
                            
                            await _hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", "delete", id);
                            
                            _logger.LogWarning("✅ SignalR delete notification sent successfully for CategoryId: {CategoryId}", id);
                            Console.WriteLine($"✅ SignalR delete notification sent successfully for CategoryId: {id}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. CategoryId: {Id}", id);
                        }

                        return successResponse;
                    }

                    // If API returns success
                    if (result.Success)
                    {
                        try
                        {
                            _logger.LogWarning("⭐ Sending DELETE notification via SignalR for CategoryId: {CategoryId} ⭐", id);
                            Console.WriteLine($"⭐ Sending DELETE notification via SignalR for CategoryId: {id} ⭐");
                            
                            await _hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", "delete", id);
                            
                            _logger.LogWarning("✅ SignalR delete notification sent successfully for CategoryId: {CategoryId}", id);
                            Console.WriteLine($"✅ SignalR delete notification sent successfully for CategoryId: {id}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "⚠️ Failed to send SignalR notification for delete. CategoryId: {Id}", id);
                            Console.WriteLine($"⚠️ Failed to send SignalR notification for delete. CategoryId: {id}, Error: {signalREx.Message}");
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
                        Message = "Category deleted successfully",
                        Data = true
                    };

                    // Send SignalR notification
                    try
                    {
                        _logger.LogWarning("⭐ Sending DELETE notification via SignalR for CategoryId: {CategoryId} ⭐", id);
                        Console.WriteLine($"⭐ Sending DELETE notification via SignalR for CategoryId: {id} ⭐");
                        
                        await _hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", "delete", id);
                        
                        _logger.LogWarning("✅ SignalR delete notification sent successfully for CategoryId: {CategoryId}", id);
                        Console.WriteLine($"✅ SignalR delete notification sent successfully for CategoryId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. CategoryId: {Id}", id);
                    }

                    return successResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete operation for CategoryId: {Id}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting category",
                    Data = false,
                    Errors = new[] { ex.Message }
                };
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"HandleResponse content: {content}");
            
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
                Console.WriteLine($"Deserialized result: Success={result?.Success}, Message={result?.Message}");

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
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Error parsing response",
                    Errors = new[] { ex.Message }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing response: {ex.Message}");
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
