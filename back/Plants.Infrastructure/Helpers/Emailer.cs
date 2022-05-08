using Microsoft.Extensions.Logging;
using Plants.Application.Contracts;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Helpers
{
    public class Emailer : IEmailer
    {
        private readonly ILogger<Emailer> _logger;

        public Emailer(ILogger<Emailer> logger)
        {
            _logger = logger;
        }

        public Task SendInvitationEmail(string address, string login, string tempPassword)
        {
            _logger.LogCritical("Creating user {0} with password {1}", login, tempPassword);
            return Task.CompletedTask;
        }
    }
}
