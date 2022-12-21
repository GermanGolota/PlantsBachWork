using Plants.Shared;

namespace Plants.Application.Contracts;

public interface IJWTokenManager
{
    string CreateToken(string username, string password, UserRole[] roles);
}