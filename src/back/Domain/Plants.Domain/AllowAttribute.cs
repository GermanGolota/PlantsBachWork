using Plants.Core;

namespace Plants.Domain;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
/// <summary>
/// Apply to aggregates to allow some access
/// </summary>
public sealed class AllowAttribute : Attribute
{

    public AllowAttribute(UserRole role, AllowType type)
    {
        Type = type;
        Role = role;
    }

    public AllowType Type { get; }
    public UserRole Role { get; }
}

public enum AllowType
{
    Read, Write
}
