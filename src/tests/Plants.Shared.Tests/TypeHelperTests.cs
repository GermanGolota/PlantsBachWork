using System.Reflection;
using Plants.Shared.Helper;

namespace Plants.Shared.Tests;

public class TypeHelperTests
{
    [Fact]
    public void LoadAssemblies_ShouldLoadThisAssembly()
    {
        var assemblies = TypeHelper.LoadPlantAssemblies(Assembly.GetExecutingAssembly()).Distinct().ToList();

        assemblies.Should().NotBeEmpty();
        assemblies.Should().Contain(Assembly.Load(this.GetType().Assembly.GetName()));
    }

    [Fact]
    public void ctor_ShouldLoadThisType()
    {
        var sut = new TypeHelper(Assembly.GetExecutingAssembly());

        sut.Types.Should().NotBeEmpty();
        sut.Types.Should().Contain(typeof(TypeHelperTests));
    }
}