using Plants.Aggregates.Services;
using Plants.Core;

namespace Plants.Aggregates.Infrastructure.Services;

internal class UserChanger : IUserChanger
{
    public Task Change(string username, UserRole role)
    {
        //TODO: Implement
        return Task.CompletedTask;
    }

    public Task Create(string username, UserRole[] roles)
    {
        //TODO: Implement
        return Task.CompletedTask;
    }
}
