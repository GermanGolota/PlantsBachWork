namespace Plants.Aggregates.Services;

public interface IIdentityHelper
{
    IUserIdentity Build(string password, string username, UserRole[] roles);
}
