namespace Plants.Aggregates.Services;

public interface IEmailer
{
    Task SendInvitationEmail(string email, string login, string tempPassword, string lang);
}
