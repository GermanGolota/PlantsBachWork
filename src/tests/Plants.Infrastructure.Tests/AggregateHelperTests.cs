using Plants.Infrastructure.Domain.Helpers;
using System.Reflection;

namespace Plants.Infrastructure.Tests;

public class AggregateHelperTests
{
    [Fact]
    public void ctor_ShouldLoadSomeCtors()
    {
        var typeHelper = new TypeHelper(Assembly.GetExecutingAssembly());
        var sut = new AggregateHelper(typeHelper);

        sut.AggregateCtors.Should().NotBeEmpty();
    }
}
