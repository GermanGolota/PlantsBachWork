using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IJWTokenManager
    {
        string CreateToken(string username, string password, CancellationToken cancellation);
    }
}