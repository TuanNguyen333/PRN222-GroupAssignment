using System.Text.Json;
using eStore.Models;
using Microsoft.Extensions.Logging;

namespace eStore.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderDetailService> _logger;

        public OrderDetailService(
            HttpClient httpClient,
            ILogger<OrderDetailService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResponse<OrderDetailDto>>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null,
            int? minQuantity = null,
            int? maxQuantity = null,
            double? minDiscount = null,
            double? maxDiscount = null)
        {
            try
            {
                var queryParams = new List<string>();

                // Always add pagination parameters
                queryParams.Add($"PageNumber={pageNumber}");
                queryParams.Add($"PageSize={pageSize}");

                // Add filter parameters only if they have values
                if (minUnitPrice.HasValue)
                {
                    queryParams.Add($"MinUnitPrice={minUnitPrice.Value}");
                }

                if (maxUnitPrice.HasValue)
                {
                    queryParams.Add($"MaxUnitPrice={maxUnitPrice.Value}");
                }

                if (minQuantity.HasValue)
                {
                    queryParams.Add($"MinQuantity={minQuantity.Value}");
                }

                if (maxQuantity.HasValue)
                {
                    queryParams.Add($"MaxQuantity={maxQuantity.Value}");
                }

                if (minDiscount.HasValue)
                {
                    queryParams.Add($"MinDiscount={minDiscount.Value}");
                }

                if (maxDiscount.HasValue)
                {
                    queryParams.Add($"MaxDiscount={maxDiscount.Value}");
                }

                var url = $"api/orderdetails?{string.Join("&", queryParams)}";
                _logger.LogInformation("Requesting URL: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response content: {Content}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned error status code: {StatusCode}", response.StatusCode);
                    return new ApiResponse<PagedResponse<OrderDetailDto>>
                    {
                        Success = false,
                        Message = $"API returned error status code: {response.StatusCode}",
                        Data = null,
                        Errors = new[] { content }
                    };
                }
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<OrderDetailDto>>>(content, options);

                if (result == null)
                {
                    _logger.LogWarning("Failed to deserialize response");
                    return new ApiResponse<PagedResponse<OrderDetailDto>>
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
                    _logger.LogInformation("Total Items: {TotalItems}, Total Pages: {TotalPages}, Current Page: {PageNumber}",
                        result.Data.TotalItems, result.Data.TotalPages, result.Data.PageNumber);
                    if (result.Data.Items != null)
                    {
                        _logger.LogInformation("Items count: {Count}", result.Data.Items.Count);
                    }
                }

                // Ensure pagination data is properly set even when no items are found
                if (result.Data?.Items == null || !result.Data.Items.Any())
                {
                    result.Data = new PagedResponse<OrderDetailDto>
                    {
                        Items = new List<OrderDetailDto>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalItems = 0,
                        TotalPages = 0
                    };
                }
                else
                {
                    // Ensure pagination properties are set correctly
                    result.Data.PageNumber = pageNumber;
                    result.Data.PageSize = pageSize;
                    if (result.Data.TotalPages == 0 && result.Data.TotalItems > 0)
                    {
                        result.Data.TotalPages = (int)Math.Ceiling(result.Data.TotalItems / (double)pageSize);
                    }
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching order details");
                return new ApiResponse<PagedResponse<OrderDetailDto>>
                {
                    Success = false,
                    Message = "Network error while fetching order details",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details");
                return new ApiResponse<PagedResponse<OrderDetailDto>>
                {
                    Success = false,
                    Message = "Error retrieving order details",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDetailDto>> GetByIdAsync(int orderId, int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/orderdetails/{orderId}/{productId}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetByIdAsync response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = $"API returned status code: {response.StatusCode}",
                        Data = null,
                        Errors = new[] { content }
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<OrderDetailDto>>(content, options);
                    Console.WriteLine($"Deserialized result: Success={result?.Success}, Message={result?.Message}, Data={result?.Data != null}");

                    if (result == null)
                    {
                        _logger.LogWarning("Failed to deserialize order detail response for OrderId: {OrderId}, ProductId: {ProductId}", orderId, productId);
                        return new ApiResponse<OrderDetailDto>
                        {
                            Success = false,
                            Message = "Failed to deserialize response",
                            Data = null,
                            Errors = new[] { "Invalid response format" }
                        };
                    }

                    if (!result.Success || result.Data == null)
                    {
                        _logger.LogWarning("Order detail not found or API returned unsuccessful response for OrderId: {OrderId}, ProductId: {ProductId}", orderId, productId);
                        return new ApiResponse<OrderDetailDto>
                        {
                            Success = false,
                            Message = result.Message ?? "Order detail not found",
                            Data = null,
                            Errors = result.Errors ?? new[] { $"No order detail exists with OrderId {orderId} and ProductId {productId}" }
                        };
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error for OrderId: {OrderId}, ProductId: {ProductId}. Content: {Content}", orderId, productId, content);
                    return new ApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = "Error parsing order detail data",
                        Data = null,
                        Errors = new[] { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order detail with OrderId: {OrderId}, ProductId: {ProductId}", orderId, productId);
                return new ApiResponse<OrderDetailDto>
                {
                    Success = false,
                    Message = "Error retrieving order detail",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDetailDto>> CreateAsync(OrderDetailDto orderDetail)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/orderdetails", orderDetail);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = $"API returned error status code: {response.StatusCode}",
                        Data = null,
                        Errors = new[] { content }
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<OrderDetailDto>>(content, options);

                if (result == null)
                {
                    return new ApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Data = null,
                        Errors = new[] { "Invalid response format" }
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order detail");
                return new ApiResponse<OrderDetailDto>
                {
                    Success = false,
                    Message = "Error creating order detail",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDetailDto>> UpdateAsync(int orderId, int productId, OrderDetailDto orderDetail)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/orderdetails/{orderId}/{productId}", orderDetail);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = $"API returned error status code: {response.StatusCode}",
                        Data = null,
                        Errors = new[] { content }
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<OrderDetailDto>>(content, options);

                if (result == null)
                {
                    return new ApiResponse<OrderDetailDto>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Data = null,
                        Errors = new[] { "Invalid response format" }
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order detail");
                return new ApiResponse<OrderDetailDto>
                {
                    Success = false,
                    Message = "Error updating order detail",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int orderId, int productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/orderdetails/{orderId}/{productId}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"API returned error status code: {response.StatusCode}",
                        Data = false,
                        Errors = new[] { content }
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, options);

                if (result == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Data = false,
                        Errors = new[] { "Invalid response format" }
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order detail");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting order detail",
                    Data = false,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<OrderDetailDto>>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Getting order details for order {OrderId}", orderId);
                var response = await _httpClient.GetAsync($"api/orderdetails/order/{orderId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get order details. Status code: {StatusCode}", response.StatusCode);
                    return new ApiResponse<PagedResponse<OrderDetailDto>>
                    {
                        Success = false,
                        Message = $"Failed to get order details. Status code: {response.StatusCode}",
                        Errors = null
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<OrderDetailDto>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogWarning("Failed to deserialize response");
                    return new ApiResponse<PagedResponse<OrderDetailDto>>
                    {
                        Success = false,
                        Message = "Failed to deserialize response",
                        Errors = null
                    };
                }

                _logger.LogInformation("Successfully retrieved order details for order {OrderId}", orderId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for order {OrderId}", orderId);
                return new ApiResponse<PagedResponse<OrderDetailDto>>
                {
                    Success = false,
                    Message = "An error occurred while getting order details",
                    Errors = null
                };
            }
        }

        public async Task<byte[]> ExportToExcelAsync(
            int? pageNumber = null,
            int? pageSize = null,
            decimal? minUnitPrice = null,
            decimal? maxUnitPrice = null,
            int? minQuantity = null,
            int? maxQuantity = null,
            double? minDiscount = null,
            double? maxDiscount = null)
        {
            try
            {
                var url = "api/orderdetails/export";
                Console.WriteLine($"Making direct export request to: {url}");
                
                // Use a direct HTTP GET request instead of a custom message
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                Console.WriteLine($"Export API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Export API error: {response.StatusCode}, Content: {errorContent}");
                    throw new Exception($"Export failed with status code {response.StatusCode}: {errorContent}");
                }
                
                // Read the file bytes directly
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                Console.WriteLine($"Received file size: {fileBytes.Length} bytes");
                
                if (fileBytes.Length == 0)
                {
                    throw new Exception("Received empty file from server");
                }
                
                return fileBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}

