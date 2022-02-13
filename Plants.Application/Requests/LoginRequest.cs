using MediatR;

#nullable enable
namespace Plants.Application.Requests
{
    public record LoginRequest(string Login, string Password) : IRequest<LoginResult>;

    public record LoginResult(bool IsSuccessfull, string? Token)
    {
        public LoginResult() : this(false, null) { }

        public LoginResult(string token) : this(true, token) { }
    }
}
