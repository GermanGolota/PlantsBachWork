using Plants.Shared;

namespace Plants.Aggregates.Services;

public interface IUserUpdater
{
    Task Create(string username, string password, string fullName, UserRole[] roles);
    Task ChangeRole(string username, string fullName, UserRole[] oldRoles, UserRole changedRole);
    Task UpdatePassword(string username, string oldPassword, string newPassword);
}
