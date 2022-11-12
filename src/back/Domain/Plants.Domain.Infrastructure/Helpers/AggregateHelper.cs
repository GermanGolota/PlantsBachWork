using Plants.Domain;
using Plants.Shared;
using System.Reflection;

namespace Plants.Infrastructure.Domain.Helpers;

public class AggregateHelper
{
    public IReadOnlyDictionary<Type, ConstructorInfo> AggregateCtors { get; }
    public IReadOnlyDictionary<string, Type> Aggregates { get; }
    public AggregateHelper(TypeHelper helper)
    {
        Dictionary<Type, ConstructorInfo> ctors = new();
        Dictionary<string, Type> aggregates = new();
        List<Exception> exceptions = new List<Exception>();
        foreach (var type in helper.Types)
        {
            var baseType = typeof(AggregateBase);
            if (type.IsAssignableTo(baseType) && type != baseType)
            {
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null, new Type[1] { typeof(Guid) }, new[] { new ParameterModifier(1) });
                if (ctor is not null)
                {
                    ctors.Add(type, ctor);
                }
                else
                {
                    exceptions.Add(new Exception($"Please, create guid ctor for '{type.FullName}'. It is needed for persistence"));
                }
                aggregates.Add(type.Name, type);
            }
        }
        if (exceptions.Any())
        {
            throw new AggregateException("Failed to load some aggregates", exceptions);
        }
        AggregateCtors = ctors;
        Aggregates = aggregates;
    }
}
