using System.Net.Http.Json;
using System.Text;
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

        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            try
            {
                _logger.LogInformation("Creating new category: {CategoryName}", category.CategoryName);

                var json = JsonSerializer.Serialize(category);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/categories", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Create Category API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Create Category API Response Content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<Category>>(responseContent, _jsonOptions);
                    if (apiResponse?.Data == null)
                    {
                        _logger.LogWarning("API returned null data for created category");
                        return null;
                    }

                    _logger.LogInformation("Successfully created category with ID {Id}", apiResponse.Data.CategoryId);
                    return apiResponse.Data;
                }

                _logger.LogError("Error creating category. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<Category?> UpdateCategoryAsync(int id, Category category)
        {
            try
            {
                _logger.LogInformation("Updating category with ID {Id}", id);

                var json = JsonSerializer.Serialize(category);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/categories/{id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Update Category API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Update Category API Response Content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<Category>>(responseContent, _jsonOptions);
                    if (apiResponse?.Data == null)
                    {
                        _logger.LogWarning("API returned null data for updated category ID {Id}", id);
                        return null;
                    }

                    _logger.LogInformation("Successfully updated category {Id}", id);
                    return apiResponse.Data;
                }

                _logger.LogError("Error updating category. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting category with ID {Id}", id);
                var response = await _httpClient.DeleteAsync($"api/categories/{id}");

                _logger.LogInformation("Delete Category API Response Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted category {Id}", id);
                    return true;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error deleting category. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, content);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {Id}", id);
                throw;
            }
        }
    }
}