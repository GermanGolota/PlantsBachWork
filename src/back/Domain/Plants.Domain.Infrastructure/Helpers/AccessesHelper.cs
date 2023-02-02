using System.Reflection;

namespace Plants.Domain.Infrastructure;

public class AccessesHelper
{
    public Dictionary<string, Dictionary<UserRole, List<AllowType>>> AggregateAccesses { get; }
    public Dictionary<string, List<UserRole>> AggregateToWriteRoles { get; }
    public List<(string Aggregate, UserRole Role, List<AllowType> Allow)> Flat { get; }
    public Dictionary<UserRole, List<string>> RoleToAggregate { get; }

    public AccessesHelper(TypeHelper helper)
    {
        AggregateAccesses = helper.Types
          .Where(x => x.IsStrictlyAssignableTo(typeof(AggregateBase)))
          .ToDictionary(
              type => type.Name,
              type => type.GetCustomAttributes<AllowAttribute>()
                  .Select(attribute => (attribute.Role, attribute.Type))
                  .GroupBy(x => x.Role)
                  .ToDictionary(x => x.Key, x => x.Select(x => x.Type).Distinct().ToList())
          );
        AggregateToWriteRoles = AggregateAccesses.ToDictionary(
            _ => _.Key,
            x => x.Value.Where(accesses => accesses.Value.Contains(AllowType.Write)).Select(x => x.Key).Distinct().ToList()
            );
        Flat = AggregateAccesses.SelectMany(pair => pair.Value.Select(pair2 => (Aggregate: pair.Key, Role: pair2.Key, Allow: pair2.Value))).ToList();
        RoleToAggregate = Flat.GroupBy(x => x.Role).ToDictionary(x => x.Key, x => x.Select(y => y.Aggregate).ToList());
    }
}
