using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Plants.Aggregates.Infrastructure;

internal class IdentityProvider : IIdentityProvider
{
    public IdentityProvider(IHttpContextAccessor contextAccessor)
    {
        try
        {
            var user = contextAccessor.HttpContext?.User;
            if (user is not null)
            {
                var claims = user.Claims;
                var roleNames = Enum.GetNames<UserRole>();
                _identity = new UserIdentity(
                    claims.Select(_ => _.Type).Where(type => roleNames.Contains(type)).Select(Enum.Parse<UserRole>).ToArray(),
                    claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value,
                    claims.First(x => x.Type == ClaimTypes.Hash).Value);
            }
            else
            {
                _identity = null;
            }
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
            _identity = new UserIdentity(newIdentity.Roles, newIdentity.UserName, newIdentity.Hash);
        }
    }
}

internal class UserIdentity : IUserIdentity
{
    public UserIdentity(UserRole[] roles, string userName, string hash)
    {
        Roles = roles;
        UserName = userName;
        Hash = hash;
    }

    public UserRole[] Roles { get; set; }
    public string UserName { get; set; }
    public string Hash { get; set; }
}
