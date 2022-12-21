using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Plants.Application.Contracts;
using Plants.Infrastructure.Config;
using Plants.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Plants.Infrastructure.Helpers;

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

    public string CreateToken(string username, string password, UserRole[] roles)
    {
        var encPassword = _encrypter.Encrypt(password);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Hash, encPassword),
            new Claim(ClaimTypes.NameIdentifier, username)
        };
        claims.AddRange(roles.Select(x => new Claim(x.ToString(), "member")));
        return CreateTokenForClaims(claims);
    }

    private string CreateTokenForClaims(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(_config.AuthKey);
        var expires = DateTime.UtcNow.AddHours(_config.TokenValidityHours);
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
