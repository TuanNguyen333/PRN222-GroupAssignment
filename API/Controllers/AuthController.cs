using BusinessObjects.Base;
using BusinessObjects.Dto.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Security;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IAuthService _authService;

        public AuthController(IMemberService memberService, IAuthService authService)
        {
            _memberService = memberService;
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Me()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return BadRequest(ApiResponse<AuthenticationResponse>.ErrorResponse("Cannot parse User ID", new ErrorResponse("INTERNAL_SERVER_ERROR", "Cannot parse User ID")));
            }
            var response = await _memberService.GetByIdAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        //[HttpPost("register")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        //{
        //    var response = await _authService.RegisterAsync(registerDto);
        //    if (!response.Success)
        //    {
        //        return BadRequest(response);
        //    }
        //    return CreatedAtAction(nameof(GetById), new { id = response.Data.MemberId }, response);
        //}
    }
}
