using API.Controllers;
using BusinessObjects.Dto.OrderDetail;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects.Base;
using Microsoft.AspNetCore.Http;
using Xunit;

public class OrderDetailControllerTests
{
    private readonly Mock<IOrderDetailService> _mockOrderDetailService;
    private readonly OrderDetailController _controller;

    public OrderDetailControllerTests()
    {
        _mockOrderDetailService = new Mock<IOrderDetailService>();
        _controller = new OrderDetailController(_mockOrderDetailService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagedResponse = new PagedResponse<OrderDetailDto>(new List<OrderDetailDto>(), 1, 10, 0);
        var response = new PagedApiResponse<OrderDetailDto> { Success = true, Data = pagedResponse };
        _mockOrderDetailService.Setup(service => service.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<double?>(), It.IsAny<double?>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenOrderDetailExists()
    {
        // Arrange
        var response = new ApiResponse<OrderDetailDto> { Success = true, Data = new OrderDetailDto { OrderId = 1, ProductId = 1 } };
        _mockOrderDetailService.Setup(service => service.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDetailDoesNotExist()
    {
        // Arrange
        var response = new ApiResponse<OrderDetailDto> { Success = false };
        _mockOrderDetailService.Setup(service => service.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<OrderDetailForCreationResponseDto> 
        { 
            Success = true, 
            Data = new OrderDetailForCreationResponseDto { OrderId = 1, ProductId = 1 } 
        };
        _mockOrderDetailService.Setup(service => service.CreateAsync(It.IsAny<OrderDetailForCreationDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Create(new OrderDetailForCreationDto());

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<OrderDetailDto> { Success = true };
        _mockOrderDetailService.Setup(service => service.UpdateAsync(It.IsAny<int>(), It.IsAny<OrderDetailForUpdateDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Update(1, new OrderDetailForUpdateDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<string> { Success = true };
        _mockOrderDetailService.Setup(service => service.DeleteAsync(It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task ExportToExcel_ReturnsFileResult_WhenSuccess()
    {
        // Arrange
        var stream = new MemoryStream();
        var fileResult = new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        _mockOrderDetailService.Setup(service => service.ExportOrderDetailsToExcel(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<double?>(), It.IsAny<double?>())).ReturnsAsync(fileResult);

        // Act
        var result = await _controller.ExportToExcel();

        // Assert
        var fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileContentResult.ContentType);
    }

    [Fact]
    public async Task GetByOrderId_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagedResponse = new PagedResponse<OrderDetailDto>(new List<OrderDetailDto>(), 1, 10, 0);
        var response = new PagedApiResponse<OrderDetailDto> { Success = true, Data = pagedResponse };
        _mockOrderDetailService.Setup(service => service.GetByOrderIdAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetByOrderId(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }
}