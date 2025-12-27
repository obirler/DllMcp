using DllMcp.Api.Services;
using FluentAssertions;
using System.Reflection;

namespace DllMcp.Tests.Services;

public class Ecma335IdGeneratorTests
{
    [Fact]
    public void GetId_ForType_ReturnsCorrectFormat()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var id = Ecma335IdGenerator.GetId(type);

        // Assert
        id.Should().Be("T:System.String");
    }

    [Fact]
    public void GetId_ForMethod_ReturnsCorrectFormat()
    {
        // Arrange
        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

        // Act
        var id = Ecma335IdGenerator.GetId(method);

        // Assert
        id.Should().StartWith("M:System.String.Contains(System.String)");
    }

    [Fact]
    public void GetId_ForProperty_ReturnsCorrectFormat()
    {
        // Arrange
        var property = typeof(string).GetProperty("Length")!;

        // Act
        var id = Ecma335IdGenerator.GetId(property);

        // Assert
        id.Should().Be("P:System.String.Length");
    }

    [Fact]
    public void GetId_ForField_ReturnsCorrectFormat()
    {
        // Arrange
        var field = typeof(string).GetField("Empty")!;

        // Act
        var id = Ecma335IdGenerator.GetId(field);

        // Assert
        id.Should().Be("F:System.String.Empty");
    }

    [Theory]
    [InlineData(typeof(int), "T:System.Int32")]
    [InlineData(typeof(string), "T:System.String")]
    [InlineData(typeof(List<string>), "T:System.Collections.Generic.List{System.String}")]
    public void GetId_ForVariousTypes_ReturnsExpectedFormat(Type type, string expectedPrefix)
    {
        // Act
        var id = Ecma335IdGenerator.GetId(type);

        // Assert
        id.Should().Contain(expectedPrefix);
    }
}
