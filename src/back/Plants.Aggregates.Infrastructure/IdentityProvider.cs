using Microsoft.AspNetCore.Http;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Core;
using System.Security.Claims;

namespace Plants.Aggregates.Infrastructure;

internal class IdentityProvider : IIdentityProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly SymmetricEncrypter _encrypter;

    public IdentityProvider(IHttpContextAccessor contextAccessor, SymmetricEncrypter encrypter)
    {
        _contextAccessor = contextAccessor;
        _encrypter = encrypter;
    }

    public IUserIdentity Identity
    {
        get
        {
            /*var claims = _contextAccessor?.HttpContext?.User?.Claims ?? throw new Exception("User is not authorized");
            var userName = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var roleNames = Enum.GetNames<UserRole>();
            var roles = claims.Select(_ => _.Type).Where(type => roleNames.Contains(type)).Select(Enum.Parse<UserRole>).ToArray();
            var hash = claims.First(x => x.Type == ClaimTypes.Hash).Value;
            return new UserIdentity(roles, userName, hash);*/
            return new UserIdentity(new[]
            {
                UserRole.Consumer,
                UserRole.Producer,
                UserRole.Manager
            }, "root", _encrypter.Encrypt("password"));
        }
    }

}

internal class UserIdentity : IUserIdentity
{
    private readonly UserRole[] _roles;
    private readonly string _userName;
    private readonly string _hash;

    public UserIdentity(UserRole[] roles, string userName, string hash)
    {
        _roles = roles;
        _userName = userName;
        _hash = hash;
    }

    public UserRole[] Roles => _roles;

    public string UserName => _userName;

    public string Hash => _hash;
}
