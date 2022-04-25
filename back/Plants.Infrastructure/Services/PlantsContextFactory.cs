using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Plants.Infrastructure.Config;
using System;
using System.Linq;
using System.Security.Claims;

namespace Plants.Infrastructure.Services
{
    public class PlantsContextFactory
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly SymmetricEncrypter _encrypter;
        private readonly AuthConfig _config;

        public PlantsContextFactory(IHttpContextAccessor httpContext, IOptions<AuthConfig> options, SymmetricEncrypter encrypter)
        {
            _httpContext = httpContext;
            _encrypter = encrypter;
            _config = options.Value;
        }

        public PlantsContext CreateDbContext()
        {
            var claims = _httpContext.HttpContext.User.Claims;

            var loginClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var pass = claims.FirstOrDefault(x => x.Type == ClaimTypes.Hash);
            if (loginClaim != default && pass != default)
            {
                var actualPass = _encrypter.Decrypt(pass.Value);
                var login = loginClaim.Value;
                return CreateFromCreds(login, actualPass);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }

        public PlantsContext CreateFromCreds(string login, string pass)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlantsContext>();
            var connectionStr = String.Format(_config.ConnectionTemplate, login, pass);
            optionsBuilder.UseNpgsql(connectionStr);
            return new PlantsContext(optionsBuilder.Options);
        }
    }
}
