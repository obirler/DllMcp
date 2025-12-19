# DllMcp.Tests

This is the test project for the MCP-DLL Document Proxy Server.

## Test Structure

The test project is organized into three main categories:

### 1. Services Tests (`Services/`)

Unit tests for service layer components:

- **Ecma335IdGeneratorTests.cs** - Tests for ECMA-335 documentation ID generation
  - Type ID generation
  - Method ID generation with parameters
  - Property and field ID generation
  - Generic type support

- **XmlDocumentationParserTests.cs** - Tests for XML documentation parsing
  - Loading XML documentation files
  - Parsing summary, remarks, returns, examples
  - Parameter and exception documentation
  - Missing documentation handling

### 2. Data Tests (`Data/`)

Unit tests for data access layer:

- **AssemblyRepositoryTests.cs** - Tests for repository operations
  - Assembly CRUD operations
  - Type and member persistence
  - Search functionality
  - Pagination logic
  - Uses in-memory database for isolation

### 3. Integration Tests (`Integration/`)

End-to-end API tests:

- **ApiIntegrationTests.cs** - Tests for API endpoints
  - Assembly management endpoints
  - Type listing with search and pagination
  - Member details retrieval
  - MCP tool endpoints (dll.listTypes, dll.getMemberDetails)
  - HTTP status codes and response formats
  - Uses WebApplicationFactory for realistic testing

## Running the Tests

### Run all tests
```bash
dotnet test
```

### Run tests with detailed output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~Ecma335IdGeneratorTests"
```

### Run tests by category
```bash
# Run only unit tests
dotnet test --filter "FullyQualifiedName~Services|FullyQualifiedName~Data"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

## Test Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library for readable tests
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing infrastructure
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for unit tests

## Test Coverage

The tests cover:
- ✅ ECMA-335 ID generation for all member types
- ✅ XML documentation parsing and extraction
- ✅ Database CRUD operations
- ✅ Search and pagination logic
- ✅ API endpoint responses
- ✅ MCP tool functionality
- ✅ Error handling and edge cases

## Writing New Tests

### Unit Test Example
```csharp
[Fact]
public void MyMethod_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    var sut = new MyClass();
    
    // Act
    var result = sut.MyMethod("input");
    
    // Assert
    result.Should().Be("expected");
}
```

### Integration Test Example
```csharp
[Fact]
public async Task GetEndpoint_ReturnsSuccessStatusCode()
{
    // Act
    var response = await _client.GetAsync("/api/endpoint");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Test Results

Current test status: **30 tests total**
- ✅ 29 passed
- ⏭️ 1 skipped (static file test requires file system setup)
- ❌ 0 failed

## Notes

- Integration tests use an in-memory database that is automatically cleaned up
- Each repository test uses a fresh database context to ensure isolation
- API integration tests use WebApplicationFactory for realistic HTTP testing
- Tests are designed to be fast and run independently
