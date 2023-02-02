using Nest;

namespace Plants.Aggregates.Infrastructure;

internal class UserSearchParamsProjector : ISearchParamsProjector<User, UserSearchParams>
{
    private readonly IIdentityProvider _identity;

    public UserSearchParamsProjector(IIdentityProvider identity)
    {
        _identity = identity;
    }

    public SearchDescriptor<User> ProjectParams(UserSearchParams parameters, SearchDescriptor<User> desc) =>
        desc.Query(
            q => q.Bool(
                b =>
                {
                    b.Must(
                        u =>
                    {
                        var name = parameters.Name;
                        if (String.IsNullOrEmpty(name) is false)
                        {
                            u.Fuzzy(f => f.Field(_ => _.FullName).Value(name));
                        }
                        else
                        {
                            u.MatchAll();
                        }
                        return u;
                    },
                    u =>
                    {
                        if (String.IsNullOrEmpty(parameters.Phone) is false)
                        {
                            u.Fuzzy(f => f.Field(_ => _.PhoneNumber).Value(parameters.Phone));
                        }
                        else
                        {
                            u.MatchAll();
                        }
                        return u;
                    },
                    u =>
                    {
                        if (parameters.Roles is not null && parameters.Roles.Length > 0)
                        {
                            var roleNames = GetVisibleRoles(parameters.Roles).ToList();
                            u.Terms(_ => _.Field(_ => _.Roles).Terms(roleNames));
                        }
                        else
                        {
                            u.MatchAll();
                        }
                        return u;
                    });
                    return b;
                }
                ));

    private UserRole[] GetVisibleRoles(UserRole[] requestedRoles)
    {
        var roles = _identity.Identity!.Roles;
        var lackingRoles = Enum.GetValues<UserRole>().Except(roles);
        var result = requestedRoles.Except(lackingRoles).ToArray();
        return result;
    }
}
