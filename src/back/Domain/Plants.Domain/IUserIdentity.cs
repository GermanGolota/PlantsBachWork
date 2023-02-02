using Plants.Shared.Model;

namespace Plants.Domain;

public interface IUserIdentity
{
    UserRole[] Roles { get; }
    string UserName { get; }
    string Hash { get; }
}
