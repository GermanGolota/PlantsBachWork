namespace Plants.Aggregates.Services;

public interface IAuthorizer
{
    Task<AuthorizeResult?> AuthorizeAsync(string username, string password, CancellationToken token = default);
}

public record AuthorizeResult(string Username, UserRole[] Roles, string Token);