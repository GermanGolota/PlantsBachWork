using MediatR;
using Plants.Core;

#nullable enable
namespace Plants.Application.Requests
{
    public record LoginRequest(string Login, string Password) : IRequest<LoginResult>;

    public record LoginResult(bool IsSuccessfull, string? Token, UserRole[]? Roles)
    {
        public LoginResult() : this(false, null, null) { }

        public LoginResult(string token, UserRole[] roles) : this(true, token, roles) { }
    }
}
