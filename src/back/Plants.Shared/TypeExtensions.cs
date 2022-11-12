namespace Plants.Shared;

public static class TypeExtensions
{
    /// <summary>
    /// Checks for assignability, excluding the type itself
    /// </summary>
    public static bool IsStrictlyAssignableToGenericType(this Type givenType, Type genericType)
    {
        bool result;
        if (givenType == genericType)
        {
            result = false;
        }
        else
        {
            result = givenType.IsAssignableToGenericType(genericType);
        }
        return result;
    }

    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        bool result;
        if (givenType == genericType
            || givenType.InheritsGenericType(genericType)
            || givenType.IsGeneticType(genericType)
            )
        {
            result = true;
        }
        else
        {
            if (givenType.BaseType is not null)
            {
                result = givenType.BaseType.IsAssignableToGenericType(genericType);
            }
            else
            {
                result = false;
            }
        }
        return result;
    }

    private static bool IsGeneticType(this Type givenType, Type genericType)
    {
        return givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType;
    }

    private static bool InheritsGenericType(this Type givenType, Type genericType)
    {
        return givenType.GetInterfaces().Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType);
    }

    public static IEnumerable<Type> GetImplementations(this Type type, Type genericType) =>
        type.GetInterfaces().Where(x => x.IsAssignableToGenericType(genericType));

}
