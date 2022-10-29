using Plants.Infrastructure.Domain.Helpers;
using System.Reflection;

namespace Plants.Infrastructure.Tests;

public class TypeHelperTests
{
    [Fact]
    public void LoadAssemblies_ShouldLoadThisAssembly()
    {
        var assemblies = TypeHelper.LoadPlantAssemblies(Assembly.GetExecutingAssembly()).Distinct().ToList();

        assemblies.Should().NotBeEmpty();
        assemblies.Should().Contain(Assembly.Load(new AssemblyName("Plants.Infrastructure.Tests")));
    }

    [Fact]
    public void ctor_ShouldLoadThisType()
    {
        var sut = new TypeHelper(Assembly.GetExecutingAssembly());

        sut.Types.Should().NotBeEmpty();
        sut.Types.Should().Contain(typeof(TypeHelperTests));
    }
}