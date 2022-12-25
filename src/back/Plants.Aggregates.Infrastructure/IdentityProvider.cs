using Microsoft.AspNetCore.Http;
using Plants.Shared;
using System.Security.Claims;

namespace Plants.Aggregates.Infrastructure;

internal class IdentityProvider : IIdentityProvider
{
    public IdentityProvider(IHttpContextAccessor contextAccessor)
    {
        try
        {
            var claims = contextAccessor.HttpContext.User.Claims;
            var roleNames = Enum.GetNames<UserRole>();
            _identity = new UserIdentity
            {
                Hash = claims.First(x => x.Type == ClaimTypes.Hash).Value,
                Roles = claims.Select(_ => _.Type).Where(type => roleNames.Contains(type)).Select(Enum.Parse<UserRole>).ToArray(),
                UserName = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value
            };
        }
        catch
        {
            _identity = null;
        }

    }

    private UserIdentity? _identity;

    public IUserIdentity? Identity => _identity;

    public void UpdateIdentity(IUserIdentity newIdentity)
    {
        if (_identity is not null)
        {
            _identity.Hash = newIdentity.Hash;
            _identity.UserName = newIdentity.UserName;
            _identity.Roles = newIdentity.Roles;
        }
        else
        {
            _identity = new UserIdentity
            {
                Hash = newIdentity.Hash,
                Roles = newIdentity.Roles,
                UserName = newIdentity.UserName
            };
        }
    }
}

internal class UserIdentity : IUserIdentity
{
    public UserRole[] Roles { get; set; }
    public string UserName { get; set; }
    public string Hash { get; set; }
}
