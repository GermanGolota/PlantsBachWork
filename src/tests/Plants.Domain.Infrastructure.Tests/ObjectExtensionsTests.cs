namespace Plants.Domain.Infrastructure.Tests;

public class ObjectExtensionsTests
{
    [Fact]
    public void RemoveProperty_ShouldRemoveProperty_WhenItExists()
    {
        //Arrange
        object obj = new
        {
            Prop1 = 1, 
            Prop2 = 2,
        };

        //Act 
        var newObj = (dynamic)obj.RemoveProperty("Prop2");

        //Assert
        Assert.Equal(1, newObj.Prop1);
        Assert.ThrowsAny<Exception>(() => newObj.Prop2);
    }
}
