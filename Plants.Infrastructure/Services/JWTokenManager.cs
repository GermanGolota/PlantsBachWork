using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Plants.Application.Contracts;
using Plants.Infrastructure.Config;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace Plants.Infrastructure.Services
{
    public class JWTokenManager : IJWTokenManager
    {
        private const string _securityAlgorithm = SecurityAlgorithms.HmacSha256Signature;
        private readonly AuthConfig _config;
        private readonly SymmetricEncrypter _encrypter;

        public JWTokenManager(IOptions<AuthConfig> config, SymmetricEncrypter encrypter)
        {
            _config = config.Value;
            _encrypter = encrypter;
        }

        public string CreateToken(string username, string password, CancellationToken cancellation)
        {
            var encPassword = _encrypter.Encrypt(password);
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Hash, encPassword),
                new Claim(ClaimTypes.NameIdentifier, username)
            };
            return CreateTokenForClaims(claims);
        }

        private string CreateTokenForClaims(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(_config.AuthKey);
            var expires = DateTime.UtcNow.AddDays(1);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), _securityAlgorithm);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = credentials
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
