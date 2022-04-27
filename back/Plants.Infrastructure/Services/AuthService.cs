using Npgsql;
using Plants.Application.Contracts;
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

        public CredsResponse CheckCreds(string login, string password)
        {
            PlantsContext ctx = null;
            CredsResponse result;
            try
            {
                ctx = _contextFactory.CreateFromCreds(login, password);
                var roles = ctx.CurrentUserRoles.ToList();
                result = new CredsResponse(roles.Select(x => x.RoleName).ToArray());
            }
            catch (PostgresException ex)
            {
                result = new CredsResponse();
            }
            finally
            {
                ctx?.Dispose();
            }
            return result;
        }
    }
}
