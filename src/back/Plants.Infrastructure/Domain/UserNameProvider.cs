using Microsoft.AspNetCore.Http;
using Plants.Domain.Services;
using System.Security.Claims;

namespace Plants.Infrastructure.Domain;

internal class UserNameProvider : IUserNameProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserNameProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string UserName => _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
}
