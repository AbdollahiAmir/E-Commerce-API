using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shop_API.Logics.IServices.IAuthServices;
using Shop_API.Options;

namespace Shop_API.Logics.Services.AuthServices
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;
        private readonly JsonWebTokenHandler _handler = new();

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;

            _credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256);
        }

        public (string Token, DateTime ExpiresAt) CreateAccessToken(int userId)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sub", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    new Claim("jti", Guid.NewGuid().ToString("N"))
                }),
                Expires = expiresAt,
                SigningCredentials = _credentials
            };

            return (_handler.CreateToken(descriptor), expiresAt);
        }

        public (string Token, byte[] Hash, DateTime ExpiresAt) CreateRefreshToken()
        {
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));

            return (token, HashRefreshToken(token), DateTime.UtcNow.AddDays(_options.RefreshTokenDays));
        }

        public byte[] HashRefreshToken(string token)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(token));
        }
    }
}