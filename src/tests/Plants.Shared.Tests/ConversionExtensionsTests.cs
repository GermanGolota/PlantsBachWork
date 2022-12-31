namespace Plants.Shared.Tests;

public class ConversionExtensionsTests
{
    [Fact]
    public void ToGuidThenToInt_ShouldPreserveValue()
    {
        //Arrange
        var initialValue = 123456L;

        //Act
        var guid = initialValue.ToGuid();
        var outputValue = guid.ToLong();

        //Assert
        outputValue.Should().Be(initialValue);
    }
}
