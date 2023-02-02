using Plants.Shared.Model;

namespace Plants.Aggregates.Infrastructure.Helper;

internal static class UserRoleExtensions
{
    internal static IEnumerable<UserRole> ApplyChangeInto(this UserRole role, UserRole[] roles) =>
            (roles.Contains(role) switch
            {
                true => roles.Except(new[] { role }),
                false => roles.Append(role)
            })
            .ToArray();

}
