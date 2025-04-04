using API.Controllers;
using BusinessObjects.Dto.Member;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interface;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessObjects.Base;
using Microsoft.AspNetCore.Http;
using Xunit;

public class MemberControllerTests
{
    private readonly Mock<IMemberService> _mockMemberService;
    private readonly MemberController _controller;

    public MemberControllerTests()
    {
        _mockMemberService = new Mock<IMemberService>();
        _controller = new MemberController(_mockMemberService.Object);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOkResult_WhenUserIsAuthenticated()
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

        var response = new ApiResponse<MemberDto> { Success = true, Data = new MemberDto { MemberId = 1 } };
        _mockMemberService.Setup(service => service.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagedResponse = new PagedResponse<MemberDto>(new List<MemberDto>(), 1, 10, 0);
        var response = new PagedApiResponse<MemberDto> { Success = true, Data = pagedResponse };
        _mockMemberService.Setup(service => service.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var response = new ApiResponse<MemberDto> { Success = false };
        _mockMemberService.Setup(service => service.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(response);

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
        var response = new ApiResponse<MemberDto> { Success = true, Data = new MemberDto { MemberId = 1 } };
        _mockMemberService.Setup(service => service.CreateAsync(It.IsAny<MemberForCreationDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Create(new MemberForCreationDto());

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<MemberDto> { Success = true };
        _mockMemberService.Setup(service => service.UpdateAsync(It.IsAny<int>(), It.IsAny<MemberForUpdateDto>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Update(1, new MemberForUpdateDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var response = new ApiResponse<string> { Success = true };
        _mockMemberService.Setup(service => service.DeleteAsync(It.IsAny<int>())).ReturnsAsync(response);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }
}