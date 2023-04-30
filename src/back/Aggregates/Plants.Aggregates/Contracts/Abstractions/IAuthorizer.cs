namespace Plants.Aggregates;

public interface IAuthorizer
{
    Task<AuthorizeResult?> AuthorizeAsync(string username, string password, CancellationToken token = default);
}

public record AuthorizeResult(Guid UserId, string Username, UserRole[] Roles, string Token);