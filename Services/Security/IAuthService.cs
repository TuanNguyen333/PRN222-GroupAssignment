﻿using BusinessObjects.Base;
using BusinessObjects.Dto.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Security
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthenticationResponse>> LoginAsync(LoginDto login);
        Task<ApiResponse<string>> LogoutAsync(int userId);
    }
}
