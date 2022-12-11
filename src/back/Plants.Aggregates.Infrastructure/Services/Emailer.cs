using Microsoft.Extensions.Logging;
using Plants.Aggregates.Services;

namespace Plants.Aggregates.Infrastructure.Services;

internal class Emailer : IEmailer
{
	private readonly ILogger<Emailer> _logger;

	public Emailer(ILogger<Emailer> logger)
	{
		_logger = logger;
	}

	public Task SendInvitationEmail(string email, string login, string tempPassword, string lang)
	{
		_logger.LogWarning("Emailing not implemented, creating user with password - '{pass}'", tempPassword);

		return Task.CompletedTask;
	}
}
