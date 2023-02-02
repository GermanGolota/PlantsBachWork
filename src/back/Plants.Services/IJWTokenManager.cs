using Plants.Shared.Model;

namespace Plants.Services;

public interface IJWTokenManager
{
    string CreateToken(string username, string password, UserRole[] roles);
}