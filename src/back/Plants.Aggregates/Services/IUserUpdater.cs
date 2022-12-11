using Plants.Core;

namespace Plants.Aggregates.Services;

public interface IUserUpdater
{
    Task Create(string username, UserRole[] roles);
    Task Change(string username, UserRole role);
}
