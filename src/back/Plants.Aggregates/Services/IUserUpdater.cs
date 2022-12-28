namespace Plants.Aggregates.Services;

public interface IUserUpdater
{
    Task CreateAsync(string username, string password, string fullName, UserRole[] roles);
    Task ChangeRoleAsync(string username, string fullName, UserRole[] oldRoles, UserRole changedRole);
    Task UpdatePasswordAsync(string username, string oldPassword, string newPassword);
}
