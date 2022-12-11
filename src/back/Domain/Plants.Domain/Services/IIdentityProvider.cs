namespace Plants.Domain.Services;

/// <summary>
/// Gets the identity of the caller
/// </summary>
public interface IIdentityProvider
{
    IUserIdentity Identity { get; }
}
