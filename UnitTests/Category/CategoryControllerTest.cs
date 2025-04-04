using API.Controllers;
using BusinessObjects.Dto.Category;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects.Base;
using Xunit;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _controller = new CategoryController(_mockCategoryService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagedResponse = new PagedResponse<CategoryDto>(new List<CategoryDto>(), 1, 10, 0);
        var response = new PagedApiResponse<CategoryDto> { Success = true, Data = pagedResponse };
        _mockCategoryService.Setup(service => service.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var response = new ApiResponse<CategoryDto> { Success = false };
        _mockCategoryService.Setup(service => service.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(response);

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
        var response = new ApiResponse<CategoryDto> { Success = true, Data = new CategoryDto { CategoryId = 1 } };
        _mockCategoryService.Setup(service => service.CreateAsync(It.IsAny<CategoryForCreationDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Create(new CategoryForCreationDto());

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<CategoryDto> { Success = true };
        _mockCategoryService.Setup(service => service.UpdateAsync(It.IsAny<int>(), It.IsAny<CategoryForUpdateDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Update(1, new CategoryForUpdateDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<string> { Success = true };
        _mockCategoryService.Setup(service => service.DeleteAsync(It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }
}