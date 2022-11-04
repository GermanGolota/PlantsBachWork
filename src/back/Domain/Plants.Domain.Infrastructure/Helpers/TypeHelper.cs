using System.Reflection;

namespace Plants.Infrastructure.Domain.Helpers;

public class TypeHelper
{
    public IEnumerable<Type> Types { get; }
    public IEnumerable<Assembly> Assemblies { get; }

    public TypeHelper() : this(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
    {
    }

    internal TypeHelper(Assembly root)
    {
        Assemblies = LoadPlantAssemblies(root).Distinct().ToList();
        Types = Assemblies.SelectMany(x => x.GetTypes()).Distinct().ToList();
    }

    internal static IEnumerable<Assembly> LoadPlantAssemblies(Assembly root)
    {
        var plantAssemblies = root.GetReferencedAssemblies().Where(x => x.Name is not null && x.Name.StartsWith(nameof(Plants)));
        foreach (var assemblyName in plantAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var child in LoadPlantAssemblies(assembly))
            {
                yield return child;
            }
            yield return assembly;
        }
        yield return root;
    }
}
