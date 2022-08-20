using Plants.Core;

namespace Plants.Application.Contracts
{
    public interface IJWTokenManager
    {
        string CreateToken(string username, string password, UserRole[] roles);
    }
}