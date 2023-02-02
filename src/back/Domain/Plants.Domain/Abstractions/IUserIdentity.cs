namespace Plants.Domain;

public interface IUserIdentity
{
    UserRole[] Roles { get; }
    string UserName { get; }
    string Hash { get; }
}
