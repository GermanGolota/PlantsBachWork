using Npgsql;
using Plants.Application.Contracts;
using System;
using System.Linq;

namespace Plants.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly PlantsContextFactory _contextFactory;

        public AuthService(PlantsContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public bool AreValidCreds(string login, string password)
        {
            bool result;
            PlantsContext ctx = null;
            try
            {
                ctx = _contextFactory.CreateFromCreds(login, password);
                var roles = ctx.CurrentUserRoles.ToList();
                result = true;
            }
            catch (PostgresException ex)
            {
                result = false;
            }
            finally
            {
                ctx?.Dispose();
            }
            return result;
        }
    }
}
