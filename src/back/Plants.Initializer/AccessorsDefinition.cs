using Plants.Domain;
using Plants.Shared;

public record AccessorsDefinition(
    Dictionary<string, Dictionary<UserRole, List<AllowType>>> Defined,
    IEnumerable<(string Aggregate, UserRole Role, List<AllowType> Allow)> Flat,
    Dictionary<UserRole, List<string>> RoleToAggregate
    );
