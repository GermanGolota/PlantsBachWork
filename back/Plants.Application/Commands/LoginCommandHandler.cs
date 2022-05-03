using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IJWTokenManager _tokenManager;
        private readonly IAuthService _auth;

        public LoginCommandHandler(IJWTokenManager tokenManager, IAuthService auth)
        {
            _tokenManager = tokenManager;
            _auth = auth;
        }

        public Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var (login, pass) = request;
            LoginResult result;
            var response = _auth.CheckCreds(login, pass);
            if (response.IsValid)
            {
                var roles = response.Roles!;
                var token = _tokenManager.CreateToken(login, pass, roles);
                result = new LoginResult(token, roles, login);
            }
            else
            {
                result = new LoginResult();
            }
            return Task.FromResult(result);
        }
    }
}
