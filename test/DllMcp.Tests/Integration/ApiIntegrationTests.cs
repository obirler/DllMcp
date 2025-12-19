using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using DllMcp.Api.Models;

namespace DllMcp.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAssemblies_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/assemblies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAssemblies_ReturnsJsonArray()
    {
        // Act
        var response = await _client.GetAsync("/api/assemblies");
        var assemblies = await response.Content.ReadFromJsonAsync<List<AssemblyInfo>>();

        // Assert
        assemblies.Should().NotBeNull();
        assemblies.Should().BeOfType<List<AssemblyInfo>>();
    }

    [Fact]
    public async Task GetTypes_WithInvalidAssemblyId_ReturnsEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/api/assemblies/invalid-id/types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var types = await response.Content.ReadFromJsonAsync<List<TypeInfo>>();
        types.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTypes_WithPagination_AcceptsParameters()
    {
        // Act
        var response = await _client.GetAsync("/api/assemblies/test-id/types?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTypes_WithSearch_AcceptsSearchParameter()
    {
        // Act
        var response = await _client.GetAsync("/api/assemblies/test-id/types?search=Calculator");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMember_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/members/M:Invalid.Method");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/api/assemblies")]
    [InlineData("/api/assemblies/test-id/types")]
    public async Task Endpoints_ReturnJsonContentType(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task PostMcpListTypes_WithValidPayload_ReturnsSuccess()
    {
        // Arrange
        var payload = new
        {
            assemblyId = "test-id",
            page = 1,
            pageSize = 20
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp/dll.listTypes", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostMcpGetMemberDetails_WithInvalidMemberId_ReturnsNotFound()
    {
        // Arrange
        var payload = new
        {
            memberId = "M:Invalid.Method",
            includeSource = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp/dll.getMemberDetails", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "Static file serving requires actual file system setup")]
    public async Task RootEndpoint_ReturnsStaticFile()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("MCP-DLL Document Proxy Server");
    }
}
