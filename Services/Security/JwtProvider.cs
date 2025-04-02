using BusinessObjects.Dto.Auth;
using BusinessObjects.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.Security
{
    public class JwtProvider
    {
        private readonly JwtSettings _jwtSettings;

        public JwtProvider(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public AuthenticationResponse GenerateToken(Member member)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, member.MemberId.ToString()),
            new Claim(ClaimTypes.Email, member.Email),
            new Claim(ClaimTypes.Role, "Member"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.Now.AddMinutes(_jwtSettings.ExpiryInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpirationTime = new DateTimeOffset(expiry).ToUnixTimeMilliseconds(),
            };
        }
    }
}
