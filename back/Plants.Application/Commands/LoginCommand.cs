using MediatR;
using Plants.Core;

#nullable enable
namespace Plants.Application.Commands
{
    public record LoginCommand(string Login, string Password) : IRequest<LoginResult>;

    public record LoginResult(bool IsSuccessfull, string? Token, UserRole[]? Roles, string? Username)
    {
        public LoginResult() : this(false, null, null, null) { }

        public LoginResult(string token, UserRole[] roles, string username) : this(true, token, roles, username) { }
    }
}
