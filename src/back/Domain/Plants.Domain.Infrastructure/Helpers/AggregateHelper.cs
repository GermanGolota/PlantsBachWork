using Plants.Domain;
using Plants.Shared;
using System.Reflection;

namespace Plants.Infrastructure.Domain.Helpers;

public class AggregateHelper
{
    public IReadOnlyDictionary<Type, ConstructorInfo> AggregateCtors { get; }
    public ITwoWayDictionary<string, Type> Aggregates { get; }
    public ITwoWayDictionary<string, Type> Events { get; }
    public ITwoWayDictionary<string, Type> Commands { get; }

    public AggregateHelper(TypeHelper helper)
    {
        Dictionary<Type, ConstructorInfo> ctors = new();
        Dictionary<string, Type> aggregates = new();
        Dictionary<string, Type> events = new();
        Dictionary<string, Type> commands = new();
        List<Exception> exceptions = new List<Exception>();
        foreach (var type in helper.Types)
        {
            if (type.IsStrictlyAssignableTo(typeof(AggregateBase)))
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

            if (type.IsStrictlyAssignableTo(typeof(Event)))
            {
                events.Add(type.Name, type);
            }

            if (type.IsStrictlyAssignableTo(typeof(Command)))
            {
                commands.Add(type.Name, type);
            }
        }
        if (exceptions.Any())
        {
            throw new AggregateException("Failed to load some aggregates", exceptions);
        }
        AggregateCtors = ctors;
        Aggregates = new TwoWayDictionary<string, Type>(aggregates);
        Events = new TwoWayDictionary<string, Type>(events);
        Commands = new TwoWayDictionary<string, Type>(commands);
    }
}
