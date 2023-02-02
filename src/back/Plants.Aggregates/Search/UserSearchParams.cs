using Plants.Shared.Model;

namespace Plants.Aggregates.Search;

public record UserSearchParams(string Name, string Phone, UserRole[] Roles) : ISearchParams;