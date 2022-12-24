using Plants.Shared;

namespace Plants.Aggregates.Services;

public interface IAuthorizer
{
    Task<AuthorizeResult?> Authorize(string username, string password);
}

public record AuthorizeResult(string Username, UserRole[] Roles, string Token);