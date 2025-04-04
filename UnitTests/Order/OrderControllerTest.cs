using API.Controllers;
using BusinessObjects.Dto.Order;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interface;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessObjects.Base;
using Microsoft.AspNetCore.Http;
using Xunit;

public class OrderControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrderController(_mockOrderService.Object);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenOrderExists()
    {
        // Arrange
        var response = new ApiResponse<OrderDto> { Success = true, Data = new OrderDto { OrderId = 1 } };
        _mockOrderService.Setup(service => service.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var response = new ApiResponse<OrderDto> { Success = false };
        _mockOrderService.Setup(service => service.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<OrderDto> { Success = true, Data = new OrderDto { OrderId = 1 } };
        _mockOrderService.Setup(service => service.CreateAsync(It.IsAny<OrderForCreationDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Create(new OrderForCreationDto());

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<OrderDto> { Success = true };
        _mockOrderService.Setup(service => service.UpdateAsync(It.IsAny<int>(), It.IsAny<OrderForUpdateDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Update(1, new OrderForUpdateDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<string> { Success = true };
        _mockOrderService.Setup(service => service.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Delete(1, 1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagedResponse = new PagedResponse<OrderDto>(new List<OrderDto>(), 1, 10, 0);
        var response = new PagedApiResponse<OrderDto> { Success = true, Data = pagedResponse };
        _mockOrderService.Setup(service => service.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetOrdersByUser_ReturnsOkResult_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = "1";
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var pagedResponse = new PagedResponse<OrderDto>(new List<OrderDto>(), 1, 10, 0);
        var response = new PagedApiResponse<OrderDto> { Success = true, Data = pagedResponse };
        _mockOrderService.Setup(service => service.GetOrdersByUserIdAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetOrdersByUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task ExportSalesToExcel_ReturnsFileResult_WhenSuccess()
    {
        // Arrange
        var stream = new MemoryStream();
        _mockOrderService.Setup(service => service.ExportAllOrdersToExcelAsync()).ReturnsAsync(stream);

        // Act
        var result = await _controller.ExportSalesToExcel();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
    }
}