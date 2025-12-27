using DllMcp.Api.Services;
using FluentAssertions;
using System.Xml.Linq;

namespace DllMcp.Tests.Services;

public class XmlDocumentationParserTests
{
    [Fact]
    public void LoadXmlDocumentation_WithValidFile_LoadsSuccessfully()
    {
        // Arrange
        var parser = new XmlDocumentationParser();
        var xmlContent = @"<?xml version=""1.0""?>
<doc>
    <assembly>
        <name>TestAssembly</name>
    </assembly>
    <members>
        <member name=""T:TestNamespace.TestClass"">
            <summary>Test summary</summary>
            <remarks>Test remarks</remarks>
        </member>
    </members>
</doc>";
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);

        try
        {
            // Act
            parser.LoadXmlDocumentation(tempFile);
            var (summary, remarks, _, _, _, _) = parser.GetDocumentation("T:TestNamespace.TestClass");

            // Assert
            summary.Should().Be("Test summary");
            remarks.Should().Be("Test remarks");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetDocumentation_WithNonExistentMember_ReturnsNulls()
    {
        // Arrange
        var parser = new XmlDocumentationParser();

        // Act
        var (summary, remarks, returns, example, parameters, exceptions) = 
            parser.GetDocumentation("T:NonExistent.Class");

        // Assert
        summary.Should().BeNull();
        remarks.Should().BeNull();
        returns.Should().BeNull();
        example.Should().BeNull();
        parameters.Should().BeNull();
        exceptions.Should().BeNull();
    }

    [Fact]
    public void HasDocumentation_WithExistingMember_ReturnsTrue()
    {
        // Arrange
        var parser = new XmlDocumentationParser();
        var xmlContent = @"<?xml version=""1.0""?>
<doc>
    <assembly>
        <name>TestAssembly</name>
    </assembly>
    <members>
        <member name=""T:TestNamespace.TestClass"">
            <summary>Test summary</summary>
        </member>
    </members>
</doc>";
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);

        try
        {
            parser.LoadXmlDocumentation(tempFile);

            // Act
            var hasDoc = parser.HasDocumentation("T:TestNamespace.TestClass");

            // Assert
            hasDoc.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void HasDocumentation_WithNonExistentMember_ReturnsFalse()
    {
        // Arrange
        var parser = new XmlDocumentationParser();

        // Act
        var hasDoc = parser.HasDocumentation("T:NonExistent.Class");

        // Assert
        hasDoc.Should().BeFalse();
    }

    [Fact]
    public void GetDocumentation_WithParameters_ParsesCorrectly()
    {
        // Arrange
        var parser = new XmlDocumentationParser();
        var xmlContent = @"<?xml version=""1.0""?>
<doc>
    <members>
        <member name=""M:TestNamespace.TestClass.Method"">
            <summary>Test method</summary>
            <param name=""arg1"">First argument</param>
            <param name=""arg2"">Second argument</param>
        </member>
    </members>
</doc>";
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);

        try
        {
            parser.LoadXmlDocumentation(tempFile);

            // Act
            var (_, _, _, _, parameters, _) = parser.GetDocumentation("M:TestNamespace.TestClass.Method");

            // Assert
            parameters.Should().NotBeNull();
            parameters.Should().Contain("arg1");
            parameters.Should().Contain("First argument");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
