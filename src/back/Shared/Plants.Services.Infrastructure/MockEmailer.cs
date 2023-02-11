using Microsoft.Extensions.Logging;

namespace Plants.Services.Infrastructure;

internal class MockEmailer : IEmailer
{
    private readonly ILogger<MockEmailer> _logger;

    public MockEmailer(ILogger<MockEmailer> logger)
    {
        _logger = logger;
    }

    public Task SendInvitationEmailAsync(string email, string login, string tempPassword, string lang, CancellationToken token = default)
    {
        _logger.LogWarning("Emailing not implemented, creating user with password - '{pass}'", tempPassword);

        return Task.CompletedTask;
    }
}
