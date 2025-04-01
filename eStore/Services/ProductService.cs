using System.Text.Json;
using eStore.Models;
using Microsoft.AspNetCore.SignalR;
using eStore.Hubs;
using Microsoft.Extensions.Logging;

namespace eStore.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IHubContext<ProductHub> _hubContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            HttpClient httpClient,
            IHubContext<ProductHub> hubContext,
            ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _hubContext = hubContext;
            _logger = logger;
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
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetProductByIdAsync response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<Product>
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
                    var result = JsonSerializer.Deserialize<ApiResponse<Product>>(content, options);
                    Console.WriteLine($"Deserialized result: Success={result?.Success}, Message={result?.Message}, Data={result?.Data != null}");

                    if (result == null)
                    {
                        _logger.LogWarning("Failed to deserialize product response for ID: {Id}", id);
                        return new ApiResponse<Product>
                        {
                            Success = false,
                            Message = "Failed to deserialize response",
                            Data = null,
                            Errors = new[] { "Invalid response format" }
                        };
                    }

                    if (!result.Success || result.Data == null)
                    {
                        _logger.LogWarning("Product not found or API returned unsuccessful response for ID: {Id}", id);
                        return new ApiResponse<Product>
                        {
                            Success = false,
                            Message = result.Message ?? "Product not found",
                            Data = null,
                            Errors = result.Errors ?? new[] { $"No product exists with ID {id}" }
                        };
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error for product ID: {Id}. Content: {Content}", id, content);
                    return new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Error parsing product data",
                        Data = null,
                        Errors = new[] { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {Id}", id);
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
                if (product == null)
                {
                    return new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Product cannot be null",
                        Errors = new[] { "Invalid product data" }
                    };
                }

                var response = await _httpClient.PostAsJsonAsync("api/products", product);
                var result = await HandleResponse<Product>(response);
                
                if (result.Success && result.Data != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate", "create", result.Data.ProductId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Error creating product",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Product>> UpdateProductAsync(int id, Product product)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product);
                var result = await HandleResponse<Product>(response);
                
                if (result.Success)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate", "update", id);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return new ApiResponse<Product>
                {
                    Success = false,
                    Message = "Error updating product",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/products/{id}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Delete response content: {content}");

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

                // Nếu response trống hoặc không phải JSON hợp lệ, xem như xóa thành công
                if (string.IsNullOrWhiteSpace(content))
                {
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Product deleted successfully",
                        Data = true
                    };

                    // Gửi thông báo SignalR
                    try
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate", "delete", id);
                        Console.WriteLine($"SignalR notification sent for delete. ProductId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. ProductId: {Id}", id);
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
                    Console.WriteLine($"Delete result: Success={result?.Success}, Message={result?.Message}, Data={result?.Data}");

                    if (result == null)
                    {
                        // Nếu không deserialize được nhưng status code là success, xem như xóa thành công
                        var successResponse = new ApiResponse<bool>
                        {
                            Success = true,
                            Message = "Product deleted successfully",
                            Data = true
                        };

                        // Gửi thông báo SignalR
                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate", "delete", id);
                            Console.WriteLine($"SignalR notification sent for delete. ProductId: {id}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. ProductId: {Id}", id);
                        }

                        return successResponse;
                    }

                    // Nếu API trả về thành công
                    if (result.Success)
                    {
                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate", "delete", id);
                            Console.WriteLine($"SignalR notification sent for delete. ProductId: {id}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. ProductId: {Id}", id);
                        }
                    }

                    return result;
                }
                catch (JsonException jsonEx)
                {
                    // Nếu không parse được JSON nhưng status code là success, xem như xóa thành công
                    _logger.LogWarning(jsonEx, "Could not parse delete response as JSON, but status code indicates success");
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Product deleted successfully",
                        Data = true
                    };

                    // Gửi thông báo SignalR
                    try
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate", "delete", id);
                        Console.WriteLine($"SignalR notification sent for delete. ProductId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. ProductId: {Id}", id);
                    }

                    return successResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete operation for ProductId: {Id}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting product",
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