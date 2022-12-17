﻿using Plants.Core;

namespace Plants.Domain.Extensions;

public static class IdentityExtensions
{
    public static CommandForbidden? HasRole(this IUserIdentity identity, UserRole role) =>
        identity.HasRoles(UserCheckType.All, role);

    public static CommandForbidden? HasRoles(this IUserIdentity identity, UserCheckType type, params UserRole[] roles)
    {
        var intersection = identity.Roles.Intersect(roles);
        return type switch
        {
            UserCheckType.All => intersection.SequenceEqual(roles),
            UserCheckType.Any => intersection.Any(),
            _ => throw new NotImplementedException()
        } switch
        {
            true => null,
            false => new CommandForbidden("You are not authorized to perform this action")
        };
    }

}

public enum UserCheckType
{
    All, Any
}