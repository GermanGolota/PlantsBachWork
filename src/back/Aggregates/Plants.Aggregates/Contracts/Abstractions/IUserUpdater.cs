namespace Plants.Aggregates;

public interface IUserUpdater
{
    Task CreateAsync(string username, string password, string fullName, UserRole[] roles, CancellationToken token = default);
    Task ChangeRoleAsync(string username, string fullName, UserRole[] oldRoles, UserRole changedRole, CancellationToken token = default);
    Task UpdatePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken token = default);
}
