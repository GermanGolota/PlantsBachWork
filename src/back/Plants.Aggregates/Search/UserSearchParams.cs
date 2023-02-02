namespace Plants.Aggregates;

public record UserSearchParams(string Name, string Phone, UserRole[] Roles) : ISearchParams;