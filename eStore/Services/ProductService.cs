using eStore.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace eStore.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<PagedResponse<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Fetching products with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);
                
                var response = await _httpClient.GetAsync($"api/products?pageNumber={pageNumber}&pageSize={pageSize}");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("API Response: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Product>>>(content, _jsonOptions);
                    
                    if (result?.Data == null)
                    {
                        _logger.LogWarning("API returned null data");
                        return new PagedResponse<Product> 
                        { 
                            Items = new List<Product>(),
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };
                    }

                    _logger.LogInformation("Successfully retrieved {Count} products", result.Data.Items.Count);
                    return result.Data;
                }
                
                _logger.LogError("API returned error status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"API returned status code: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching products");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing API response");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching products");
                throw;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{id}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {Id}", id);
                throw;
            }
        }

        public async Task<Product?> CreateProductAsync(Product product)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products", product);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            try
            {
                _logger.LogInformation("Sending update request: {Product}", JsonSerializer.Serialize(product));
                var response = await _httpClient.PutAsJsonAsync($"api/products/{product.ProductId}", product);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Update response: {Response}", content);
                
                var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/products/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Errors?.Message != null)
                {
                    throw new Exception(result.Errors.Message);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {Id}", id);
                throw;
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public ErrorResponse? Errors { get; set; }
    }

    public class ErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
} 