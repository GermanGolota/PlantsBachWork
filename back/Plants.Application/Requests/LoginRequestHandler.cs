using MediatR;
using Plants.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResult>
    {
        private readonly IJWTokenManager _tokenManager;
        private readonly IAuthService _auth;

        public LoginRequestHandler(IJWTokenManager tokenManager, IAuthService auth)
        {
            _tokenManager = tokenManager;
            _auth = auth;
        }

        public Task<LoginResult> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var (login, pass) = request;
            LoginResult result;
            var response = _auth.CheckCreds(login, pass);
            if (response.IsValid)
            {
                var roles = response.Roles!;
                var token = _tokenManager.CreateToken(login, pass, roles);
                result = new LoginResult(token, roles);
            }
            else
            {
                result = new LoginResult();
            }
            return Task.FromResult(result);
        }
    }
}
