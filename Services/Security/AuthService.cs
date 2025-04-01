using BusinessObjects.Base;
using BusinessObjects.Dto.Auth;
using BusinessObjects.Entities;
using Repositories.Interface;
using Services.Client.Cache;

namespace Services.Security
{
    public class AuthService : IAuthService
    {
        private readonly JwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(JwtProvider jwtProvider, IPasswordService passwordService, ICacheService cacheService, IUnitOfWork unitOfWork)
        {
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ApiResponse<AuthenticationResponse>> LoginAsync(LoginDto login)
        {
            Member? member = await _unitOfWork.MemberRepository.GetByEmailAsync(login.Email);
            //if (member == null || !_passwordService.VerifyPassword(login.Password, member.Password))
            if (member == null || login.Password != member.Password)
            {
                return ApiResponse<AuthenticationResponse>.ErrorResponse("Invalid email or password", new ErrorResponse("BAD_CREDENTIALS", "Invalid email or password"));
            }
            var response = _jwtProvider.GenerateToken(member);
            await _cacheService.SetAsync($"whitelist:{member.MemberId}", response.Token);
            return ApiResponse<AuthenticationResponse>.SuccessResponse(response, "Logged in successfully");
        }
    }
}
