using Microsoft.AspNetCore.Http;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Services;
using System.Security.Claims;

namespace Plants.Infrastructure.Domain;

internal class IdentityProvider : IIdentityProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public IdentityProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public IUserIdentity Identity
    {
        get
        {
            var claims = _contextAccessor?.HttpContext?.User?.Claims ?? throw new Exception("User is not authorized");
            var userName = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var roleNames = Enum.GetNames<UserRole>();
            var roles = claims.Select(_ => _.Type).Where(type => roleNames.Contains(type)).Select(Enum.Parse<UserRole>).ToArray();
            return new UserIdentity(roles, userName);
        }
    }

}

internal class UserIdentity : IUserIdentity
{
    private readonly UserRole[] _roles;
    private readonly string _userName;

    public UserIdentity(UserRole[] roles, string userName)
    {
        _roles = roles;
        _userName = userName;
    }

    public UserRole[] Roles => _roles;

    public string UserName => _userName;
}
