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
            Prop3 = 3
        };

        //Act 
        var newObj = (dynamic)obj.RemoveProperties("Prop2", "Prop3");

        //Assert
        Assert.Equal(1, newObj.Prop1);
        Assert.ThrowsAny<Exception>(() => newObj.Prop2);
        Assert.ThrowsAny<Exception>(() => newObj.Prop3);
    }
}
