namespace Plants.Domain.Infrastructure.Services;

public interface IServiceIdentityProvider
{
    IUserIdentity ServiceIdentity { get; }
}
