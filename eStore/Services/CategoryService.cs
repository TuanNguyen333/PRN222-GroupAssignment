using System.Net.Http.Json;
using System.Text.Json;
using eStore.Models;
using Microsoft.Extensions.Logging;

namespace eStore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CategoryService(HttpClient httpClient, ILogger<CategoryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching categories from {BaseUrl}", _httpClient.BaseAddress);
                var response = await _httpClient.GetAsync("api/categories");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Categories API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Categories API Response Content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Category>>>(content, _jsonOptions);
                    
                    if (apiResponse?.Data?.Items == null)
                    {
                        _logger.LogWarning("API returned null data for categories");
                        return new List<Category>();
                    }

                    _logger.LogInformation("Successfully retrieved {Count} categories", apiResponse.Data.Items.Count);
                    return apiResponse.Data.Items.ToList();
                }
                
                _logger.LogError("Error fetching categories. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                throw new HttpRequestException($"API returned status code: {response.StatusCode}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing categories response");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching categories");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching categories");
                throw;
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching category with ID {Id}", id);
                var response = await _httpClient.GetAsync($"api/categories/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Category API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Category API Response Content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<Category>>(content, _jsonOptions);
                    if (apiResponse?.Data == null)
                    {
                        _logger.LogWarning("API returned null data for category ID {Id}", id);
                        return null;
                    }

                    _logger.LogInformation("Successfully retrieved category {Id}", id);
                    return apiResponse.Data;
                }
                
                _logger.LogError("Error fetching category. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category with ID {Id}", id);
                return null;
            }
        }
    }
} 