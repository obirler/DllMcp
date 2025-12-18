# MCP Tools Usage Examples

This document demonstrates how to use the MCP tools provided by the DLL Document Proxy Server.

## Tool 1: dll.listTypes

Lists types from a loaded assembly with optional search and pagination.

**Request:**
```json
POST /mcp/dll.listTypes
Content-Type: application/json

{
  "assemblyId": "822e37fc-0076-4242-aa8e-294a8f9d1c55",
  "search": "Calculator",
  "page": 1,
  "pageSize": 20
}
```

**Response:**
```json
{
  "types": [
    {
      "id": "T:TestAssembly.Calculator",
      "name": "Calculator",
      "fullName": "TestAssembly.Calculator",
      "namespace": "TestAssembly",
      "kind": "Class",
      "summary": "A sample calculator class for testing the MCP-DLL Document Proxy Server."
    }
  ],
  "page": 1,
  "pageSize": 20
}
```

## Tool 2: dll.getMemberDetails

Gets detailed information about a specific member including documentation.

**Request:**
```json
POST /mcp/dll.getMemberDetails
Content-Type: application/json

{
  "memberId": "M:TestAssembly.Calculator.Add(System.Int32,System.Int32)",
  "includeSource": false
}
```

**Response:**
```json
{
  "name": "Add",
  "signature": "Int32 Add(Int32 a, Int32 b)",
  "documentation": {
    "source": "xml",
    "summary": "Adds two integers and returns the result.",
    "remarks": null,
    "returns": "The sum of the two numbers.",
    "example": "\n<code>\nvar calc = new Calculator();\nint result = calc.Add(5, 3); // Returns 8\n</code>\n"
  },
  "sourceCode": {
    "available": false,
    "language": "csharp"
  }
}
```

## Progressive Disclosure Pattern

The MCP tools follow a "Progressive Disclosure" pattern to prevent context flooding:

1. **Discovery Phase**: Use `dll.listTypes` with search/pagination to find relevant types
2. **Deep Dive Phase**: Use `dll.getMemberDetails` to get full documentation for specific members
3. **Source Analysis Phase**: Set `includeSource: true` when you need to understand implementation details

## Example Workflow

```bash
# Step 1: List all types in an assembly
curl -X POST http://localhost:5000/mcp/dll.listTypes \
  -H "Content-Type: application/json" \
  -d '{"assemblyId":"822e37fc-0076-4242-aa8e-294a8f9d1c55","pageSize":10}'

# Step 2: Search for specific types
curl -X POST http://localhost:5000/mcp/dll.listTypes \
  -H "Content-Type: application/json" \
  -d '{"assemblyId":"822e37fc-0076-4242-aa8e-294a8f9d1c55","search":"Calculator","pageSize":10}'

# Step 3: Get member details
curl -X POST http://localhost:5000/mcp/dll.getMemberDetails \
  -H "Content-Type: application/json" \
  -d '{"memberId":"M:TestAssembly.Calculator.Add(System.Int32,System.Int32)"}'

# Step 4: Get member details with source code (if needed)
curl -X POST http://localhost:5000/mcp/dll.getMemberDetails \
  -H "Content-Type: application/json" \
  -d '{"memberId":"M:TestAssembly.Calculator.Add(System.Int32,System.Int32)","includeSource":true}'
```

## Documentation Quality Indicators

The system provides three levels of documentation quality:

1. **Official (XML)** - `"source": "xml"` - Documentation from XML files
2. **AI Generated** - `"source": "ai"` - AI-generated documentation when XML is missing
3. **None** - `"source": "none"` - No documentation available (only signature)

## Pagination

For large assemblies, always use pagination:

```json
{
  "assemblyId": "...",
  "page": 1,      // Page number (1-based)
  "pageSize": 20  // Items per page (default: 20, max: 100)
}
```
