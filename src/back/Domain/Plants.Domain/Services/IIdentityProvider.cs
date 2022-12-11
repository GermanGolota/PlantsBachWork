namespace Plants.Domain.Services;

/// <summary>
/// Gets the username of the caller
/// </summary>
public interface IIdentityProvider
{
    IUserIdentity Identity { get; }
}
