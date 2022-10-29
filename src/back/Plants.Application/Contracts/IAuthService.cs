namespace Plants.Application.Contracts;

public interface IAuthService
{
    CredsResponse CheckCreds(string login, string password);
}
