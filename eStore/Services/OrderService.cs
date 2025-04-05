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
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IHttpClientFactory httpClientFactory,
            IHubContext<OrderHub> hubContext,
            ILogger<OrderService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResponse<Order>>> GetAllOrdersAsync(
       int pageNumber = 1,
       int pageSize = 10,
       decimal? minFreight = null,
       decimal? maxFreight = null,
       DateTime? minOrderDate = null,
       DateTime? maxOrderDate = null)
        {
            try
            {
                var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };
                if (minFreight.HasValue)
                {
                    queryParams.Add($"minFreight={minFreight}");
                }

                if (maxFreight.HasValue)
                {
                    queryParams.Add($"maxFreight={maxFreight}");
                }

                if (minOrderDate.HasValue)
                {
                    queryParams.Add($"minOrderDate={minOrderDate.Value:yyyy-MM-dd}");
                }

                if (maxOrderDate.HasValue)
                {
                    queryParams.Add($"maxOrderDate={maxOrderDate.Value:yyyy-MM-dd}");
                }

                var url = $"api/orders?{string.Join("&", queryParams)}";
                Console.WriteLine($"Requesting URL: {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned error status code: {response.StatusCode}");
                    return new ApiResponse<PagedResponse<Order>>
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

                var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Order>>>(content, options);

                if (result == null)
                {
                    Console.WriteLine("Failed to deserialize response");
                    return new ApiResponse<PagedResponse<Order>>
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

                // If no orders were found, return a successful response with empty list
                if (result.Data?.Items == null || !result.Data.Items.Any())
                {
                    result.Data = new PagedResponse<Order>
                    {
                        Items = new List<Order>(),
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
                return new ApiResponse<PagedResponse<Order>>
                {
                    Success = false,
                    Message = "Network error while fetching orders",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new ApiResponse<PagedResponse<Order>>
                {
                    Success = false,
                    Message = "Error retrieving orders",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Order>> GetOrderByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/orders/{id}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetOrderByIdAsync response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<Order>
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
                    var result = JsonSerializer.Deserialize<ApiResponse<Order>>(content, options);
                    Console.WriteLine($"Deserialized result: Success={result?.Success}, Message={result?.Message}, Data={result?.Data != null}");

                    if (result == null)
                    {
                        _logger.LogWarning("Failed to deserialize order response for ID: {Id}", id);
                        return new ApiResponse<Order>
                        {
                            Success = false,
                            Message = "Failed to deserialize response",
                            Data = null,
                            Errors = new[] { "Invalid response format" }
                        };
                    }

                    if (!result.Success || result.Data == null)
                    {
                        _logger.LogWarning("Order not found or API returned unsuccessful response for ID: {Id}", id);
                        return new ApiResponse<Order>
                        {
                            Success = false,
                            Message = result.Message ?? "Order not found",
                            Data = null,
                            Errors = result.Errors ?? new[] { $"No order exists with ID {id}" }
                        };
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error for order ID: {Id}. Content: {Content}", id, content);
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Error parsing order data",
                        Data = null,
                        Errors = new[] { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID: {Id}", id);
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = "Error retrieving order",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Order>> CreateOrderAsync(Order order)
        {
            try
            {
                if (order == null)
                {
                    return new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Order cannot be null",
                        Errors = new[] { "Invalid order data" }
                    };
                }

                var response = await _httpClient.PostAsJsonAsync("api/orders", order);
                var result = await HandleResponse<Order>(response);
                
                if (result.Success && result.Data != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", "create", result.Data.OrderId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = "Error creating order",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<Order>> UpdateOrderAsync(int id, Order order)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}", order);
                var result = await HandleResponse<Order>(response);
                
                if (result.Success)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", "update", id);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                return new ApiResponse<Order>
                {
                    Success = false,
                    Message = "Error updating order",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/orders/{id}");
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

                // If response is empty or not valid JSON, consider deletion successful
                if (string.IsNullOrWhiteSpace(content))
                {
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Order deleted successfully",
                        Data = true
                    };

                    // Send SignalR notification
                    try
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", "delete", id);
                        Console.WriteLine($"SignalR notification sent for delete. OrderId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. OrderId: {Id}", id);
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
                        // If can't deserialize but status code is success, consider deletion successful
                        var successResponse = new ApiResponse<bool>
                        {
                            Success = true,
                            Message = "Order deleted successfully",
                            Data = true
                        };

                        // Send SignalR notification
                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", "delete", id);
                            Console.WriteLine($"SignalR notification sent for delete. OrderId: {id}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. OrderId: {Id}", id);
                        }

                        return successResponse;
                    }

                    // If API returns success
                    if (result.Success)
                    {
                        try
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", "delete", id);
                            Console.WriteLine($"SignalR notification sent for delete. OrderId: {id}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, "Failed to send SignalR notification for delete. OrderId: {Id}", id);
                        }
                    }

                    return result;
                }
                catch (JsonException jsonEx)
                {
                    // If can't parse JSON but status code is success, consider deletion successful
                    _logger.LogWarning(jsonEx, "Could not parse delete response as JSON, but status code indicates success");
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Order deleted successfully",
                        Data = true
                    };

                    // Send SignalR notification
                    try
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", "delete", id);
                        Console.WriteLine($"SignalR notification sent for delete. OrderId: {id}");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, "Failed to send SignalR notification for delete. OrderId: {Id}", id);
                    }

                    return successResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete operation for OrderId: {Id}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting order",
                    Data = false,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<Order>>> GetUserOrdersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            decimal? minFreight = null,
            decimal? maxFreight = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (minFreight.HasValue)
                {
                    queryParams.Add($"minFreight={minFreight}");
                }

                if (maxFreight.HasValue)
                {
                    queryParams.Add($"maxFreight={maxFreight}");
                }

                if (fromDate.HasValue)
                {
                    queryParams.Add($"minOrderDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"maxOrderDate={toDate.Value:yyyy-MM-dd}");
                }

                var url = $"api/orders/user?{string.Join("&", queryParams)}";
                _logger.LogInformation($"Requesting URL: {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API returned error status code: {response.StatusCode}");
                    return new ApiResponse<PagedResponse<Order>>
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

                var result = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Order>>>(content, options);
                return result ?? new ApiResponse<PagedResponse<Order>>
                {
                    Success = false,
                    Message = "Failed to deserialize response",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders");
                return new ApiResponse<PagedResponse<Order>>
                {
                    Success = false,
                    Message = "Error getting user orders",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<byte[]> ExportAllOrdersToExcelAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/orders/export");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to export orders to Excel. Status code: {StatusCode}", response.StatusCode);
                    throw new Exception($"Failed to export orders. Server returned status code: {response.StatusCode}");
                }
                
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting orders to Excel");
                throw;
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