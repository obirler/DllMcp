using DllMcp.Api.Data;
using DllMcp.Api.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DllMcp.Tests.Data;

public class AssemblyRepositoryTests : IDisposable
{
    private readonly DllMcpDbContext _context;
    private readonly AssemblyRepository _repository;

    public AssemblyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DllMcpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DllMcpDbContext(options);
        _repository = new AssemblyRepository(_context);
    }

    [Fact]
    public async Task SaveAssemblyAsync_NewAssembly_SavesSuccessfully()
    {
        // Arrange
        var assembly = new AssemblyInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = "TestAssembly",
            HasXmlDocumentation = true,
            AssemblyPath = "/path/to/assembly.dll"
        };

        // Act
        var result = await _repository.SaveAssemblyAsync(assembly);

        // Assert
        result.Should().Be(assembly.Id);
        var saved = await _context.Assemblies.FindAsync(assembly.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("TestAssembly");
    }

    [Fact]
    public async Task GetAssembliesAsync_ReturnsAllAssemblies()
    {
        // Arrange
        var assembly1 = new AssemblyInfo { Id = "1", Name = "Assembly1", HasXmlDocumentation = true };
        var assembly2 = new AssemblyInfo { Id = "2", Name = "Assembly2", HasXmlDocumentation = false };
        
        await _repository.SaveAssemblyAsync(assembly1);
        await _repository.SaveAssemblyAsync(assembly2);

        // Act
        var assemblies = await _repository.GetAssembliesAsync();

        // Assert
        assemblies.Should().HaveCount(2);
        assemblies.Should().Contain(a => a.Name == "Assembly1");
        assemblies.Should().Contain(a => a.Name == "Assembly2");
    }

    [Fact]
    public async Task GetTypesAsync_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        var assemblyId = Guid.NewGuid().ToString();
        var assembly = new AssemblyInfo { Id = assemblyId, Name = "TestAssembly", HasXmlDocumentation = true };
        await _repository.SaveAssemblyAsync(assembly);

        var type1 = new TypeInfo
        {
            Id = "T:Test.Calculator",
            AssemblyId = assemblyId,
            Namespace = "Test",
            Name = "Calculator",
            FullName = "Test.Calculator",
            Kind = "Class"
        };

        var type2 = new TypeInfo
        {
            Id = "T:Test.StringHelper",
            AssemblyId = assemblyId,
            Namespace = "Test",
            Name = "StringHelper",
            FullName = "Test.StringHelper",
            Kind = "Class"
        };

        await _repository.SaveTypeAsync(type1);
        await _repository.SaveTypeAsync(type2);

        // Act
        var types = await _repository.GetTypesAsync(assemblyId, "Calculator");

        // Assert
        types.Should().ContainSingle();
        types.First().Name.Should().Be("Calculator");
    }

    [Fact]
    public async Task GetTypesAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var assemblyId = Guid.NewGuid().ToString();
        var assembly = new AssemblyInfo { Id = assemblyId, Name = "TestAssembly", HasXmlDocumentation = true };
        await _repository.SaveAssemblyAsync(assembly);

        // Add 5 types
        for (int i = 1; i <= 5; i++)
        {
            var type = new TypeInfo
            {
                Id = $"T:Test.Class{i}",
                AssemblyId = assemblyId,
                Namespace = "Test",
                Name = $"Class{i}",
                FullName = $"Test.Class{i}",
                Kind = "Class"
            };
            await _repository.SaveTypeAsync(type);
        }

        // Act
        var page1 = await _repository.GetTypesAsync(assemblyId, null, 1, 2);
        var page2 = await _repository.GetTypesAsync(assemblyId, null, 2, 2);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
        page1.First().Id.Should().NotBe(page2.First().Id);
    }

    [Fact]
    public async Task SaveMemberAsync_NewMember_SavesSuccessfully()
    {
        // Arrange
        var memberId = "M:Test.Class.Method";
        var member = new MemberData
        {
            Id = memberId,
            TypeId = "T:Test.Class",
            Name = "Method",
            MemberKind = "Method",
            Signature = "void Method()",
            XmlSummary = "Test summary",
            LastUpdated = DateTime.UtcNow
        };

        // Act
        await _repository.SaveMemberAsync(member);

        // Assert
        var saved = await _repository.GetMemberAsync(memberId);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Method");
        saved.XmlSummary.Should().Be("Test summary");
    }

    [Fact]
    public async Task GetAssemblyAsync_ExistingAssembly_ReturnsAssembly()
    {
        // Arrange
        var assemblyId = Guid.NewGuid().ToString();
        var assembly = new AssemblyInfo
        {
            Id = assemblyId,
            Name = "TestAssembly",
            HasXmlDocumentation = true,
            AssemblyPath = "/path/to/assembly.dll"
        };
        await _repository.SaveAssemblyAsync(assembly);

        // Act
        var result = await _repository.GetAssemblyAsync(assemblyId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestAssembly");
        result.AssemblyPath.Should().Be("/path/to/assembly.dll");
    }

    [Fact]
    public async Task GetMemberAsync_NonExistent_ReturnsNull()
    {
        // Act
        var result = await _repository.GetMemberAsync("M:NonExistent.Method");

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
