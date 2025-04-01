using System.Text.Json;
using eStore.Models;

namespace eStore.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<PagedResponse<Product>>> GetAllProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                }

                if (minUnitPrice.HasValue)
                {
                    queryParams.Add($"minUnitPrice={minUnitPrice}");
                }

                if (maxUnitPrice.HasValue)
                {
                    queryParams.Add($"maxUnitPrice={maxUnitPrice}");
                }

                var url = $"api/products?{string.Join("&", queryParams)}";
                Console.WriteLine($"Requesting URL: {url}");
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned error status code: {response.StatusCode}");
                    return new ApiResponse<PagedResponse<Product>>
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
                
                var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Product>>>(content, options);

                if (result == null)
                {
                    Console.WriteLine("Failed to deserialize response");
                    return new ApiResponse<PagedResponse<Product>>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Data = null,
                        Errors = new[] { "Invalid response format" }
                    };
                }

                Console.WriteLine($"Success: {result.Success}, Message: {result.Message}");
                if (result.Data != null)
                {
                    Console.WriteLine($"Total Items: {result.Data.TotalItems}, Total Pages: {result.Data.TotalPages}, Current Page: {result.Data.PageNumber}");
                    if (result.Data.Items != null)
                    {
                        Console.WriteLine($"Items count: {result.Data.Items.Count}");
                    }
                }

                // Nếu không có sản phẩm nào, trả về response thành công với danh sách rỗng
                if (result.Data?.Items == null || !result.Data.Items.Any())
                {
                    result.Data = new PagedResponse<Product>
                    {
                        Items = new List<Product>(),
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
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                return new ApiResponse<PagedResponse<Product>>
                {
                    Success = false,
                    Message = "Network error while fetching products",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new ApiResponse<PagedResponse<Product>>
                {
                    Success = false,
                    Message = "Error retrieving products",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Product>> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content);

                return result ?? new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Failed to deserialize response",
                    Data = null,
                    Errors = new[] { "Invalid response format" }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Error retrieving product",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Product>> CreateProductAsync(Product product)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products", product);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content);

                return result ?? new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Failed to deserialize response",
                    Data = null,
                    Errors = new[] { "Invalid response format" }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Error creating product",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Product>> UpdateProductAsync(int id, Product product)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content);

                return result ?? new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Failed to deserialize response",
                    Data = null,
                    Errors = new[] { "Invalid response format" }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Error updating product",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/products/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content);

                return result ?? new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to deserialize response",
                    Data = false,
                    Errors = new[] { "Invalid response format" }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting product",
                    Data = false,
                    Errors = new[] { ex.Message }
                };
            }
        }
    }
} 