namespace Plants.Aggregates;

public interface IIdentityHelper
{
    IUserIdentity Build(string password, string username, UserRole[] roles);
}
