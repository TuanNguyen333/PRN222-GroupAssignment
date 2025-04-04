using BusinessObjects.Base;
using BusinessObjects.Dto.Auth;
using BusinessObjects.Entities;
using Microsoft.Extensions.Configuration;
using Repositories.Interface;
using Services.Client.Cache;
using System;

namespace Services.Security
{
    public class AuthService : IAuthService
    {
        private readonly JwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(JwtProvider jwtProvider, IConfiguration configuration, IPasswordService passwordService, ICacheService cacheService, IUnitOfWork unitOfWork)
        {
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ApiResponse<AuthenticationResponse>> LoginAsync(LoginDto login)
        {
            // Check for admin credentials
            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];
            if (string.Equals(login.Email, adminEmail, StringComparison.Ordinal) &&
                string.Equals(login.Password, adminPassword, StringComparison.Ordinal))
            {
                var adminResponse = _jwtProvider.GenerateToken(0, "Admin");
                await _cacheService.SetAsync("whitelist:0", adminResponse.Token);
                return ApiResponse<AuthenticationResponse>.SuccessResponse(adminResponse, "Logged in successfully");
            }

            // Check for member credentials
            Member? member = await _unitOfWork.MemberRepository.GetByEmailAsync(login.Email);
            if (member == null || login.Password != member.Password)
            {
                return ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "Invalid email or password",
                    new ErrorResponse("BAD_CREDENTIALS", "Invalid email or password"));
            }

            var memberResponse = _jwtProvider.GenerateToken(member.MemberId, "Member");
            await _cacheService.SetAsync($"whitelist:{member.MemberId}", memberResponse.Token);
            return ApiResponse<AuthenticationResponse>.SuccessResponse(memberResponse, "Logged in successfully");
        }

        public async Task<ApiResponse<string>> LogoutAsync(int userId)
        {
            await _cacheService.DeleteAsync($"whitelist:{userId}");
            return ApiResponse<string>.SuccessResponse(null!, "Logged out successfully");
        }
    }
}
