namespace Plants.Shared.Tests;

public class ConversionExtensionsTests
{
    [Fact]
    public void ToGuidThenToLong_ShouldPreserveValue()
    {
        //Arrange
        var initialValue = 1731837033378504570;

        //Act
        var guid = initialValue.ToGuid();
        var outputValue = guid.ToLong();

        //Assert
        outputValue.Should().Be(initialValue);
    }
   
    [Fact]
    public void ToLongThenToGuid_ShouldPreserveValue_WhenSafeGuid()
    {
        //Arrange
        var initialValue = new Random().GetRandomConvertableGuid();

        //Act
        var longValue = initialValue.ToLong();
        var outputValue = longValue.ToGuid();

        //Assert
        outputValue.Should().Be(initialValue);
    }

    [Fact]
    public void ToLongThenToGuid_ShouldPreserveValue_WhenSafeGuid2()
    {
        //Arrange
        var initialValue = Guid.Parse("cfddcc1a-ed42-6a9a-0000-000000000000");

        //Act
        var longValue = initialValue.ToLong();
        var outputValue = longValue.ToGuid();

        //Assert
        outputValue.Should().Be(initialValue);
    }

}
