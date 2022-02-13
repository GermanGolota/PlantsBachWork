namespace Plants.Application.Contracts
{
    public interface IAuthService
    {
        bool AreValidCreds(string login, string password);
    }
}
