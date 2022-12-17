using Plants.Core;
using Plants.Domain;

public record AccessorsDefinition(
    Dictionary<string, Dictionary<UserRole, List<AllowType>>> Defined,
    IEnumerable<(string Aggregate, UserRole Role, List<AllowType> Allow)> Flat,
    Dictionary<UserRole, List<string>> RoleToAggregate
    );
