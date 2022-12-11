using Plants.Core;

namespace Plants.Aggregates.Services;

public interface IUserChanger
{
    Task Create(string username, UserRole[] roles);
    Task Change(string username, UserRole role);
}
