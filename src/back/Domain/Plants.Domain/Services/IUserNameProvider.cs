namespace Plants.Domain.Services;

/// <summary>
/// Gets the username of the caller
/// </summary>
public interface IUserNameProvider
{
    string UserName { get; }
}
