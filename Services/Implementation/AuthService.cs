using BusinessObjects.Base;
using BusinessObjects.Dto.Auth;
using BusinessObjects.Entities;
using Repositories.Interface;
using Services.Interface;
using Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly JwtProvider _jwtProvider;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(JwtProvider jwtProvider, IUnitOfWork unitOfWork)
        {
            _jwtProvider = jwtProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<AuthenticationResponse>> LoginAsync(LoginDto login)
        {
            Member? member = await _unitOfWork.MemberRepository.GetByEmailAsync(login.Email);
            if (member == null || member.Password != login.Password)
            {
                return ApiResponse<AuthenticationResponse>.ErrorResponse("Invalid email or password", new ErrorResponse("INVALID_CREDENTIALS", "Invalid email or password"));
            }
            return ApiResponse<AuthenticationResponse>.SuccessResponse(_jwtProvider.GenerateToken(member), "Logged in successfully");
        }
    }
}
