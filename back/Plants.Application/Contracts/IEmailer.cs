using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IEmailer
    {
        Task SendInvitationEmail(string address, string login, string tempPassword, string lang);
    }
}
