namespace Plants.Services;

public interface IEmailer
{
    Task SendInvitationEmailAsync(string email, string login, string tempPassword, string lang, CancellationToken token = default);
}
