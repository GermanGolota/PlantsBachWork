using Plants.Domain.Abstractions;
using Plants.Shared.Model;

namespace Plants.Aggregates.Abstractions;

public interface IIdentityHelper
{
    IUserIdentity Build(string password, string username, UserRole[] roles);
}
