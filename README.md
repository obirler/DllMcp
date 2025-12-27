# MCP-DLL Document Proxy Server

A comprehensive "Knowledge Gateway" for .NET assemblies that bridges the gap between compiled binary libraries and AI agents through the Model Context Protocol (MCP). This system provides a unified, queryable interface that scales from high-level documentation to low-level source code, solving the "black box" problem of compiled assemblies.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [Database Schema](#database-schema)
- [Technology Stack](#technology-stack)
- [Development](#development)
- [Testing](#testing)
- [License](#license)

## Overview

The MCP-DLL Document Proxy Server enables AI agents and developers to introspect .NET assemblies by providing structured access to:

- **Metadata**: Types, methods, properties, fields, and events extracted via Reflection
- **Documentation**: XML documentation comments mapped to code elements
- **Source Code**: On-demand decompilation of compiled methods and types
- **Search**: Fast, indexed search across namespaces, types, and members

The system implements a **4-Tier Knowledge Strategy** that progressively reveals information based on availability and need:

1. **Tier 1 (Official)**: XML documentation from the assembly
2. **Tier 2 (Augmented)**: AI-generated examples when XML lacks usage examples
3. **Tier 3 (Synthetic)**: AI-inferred documentation when XML is missing
4. **Tier 4 (Source)**: Decompiled C# source code for ultimate understanding

## Key Features

### Assembly Management
- **Upload & Indexing**: Multi-file upload supporting DLL + XML documentation pairs
- **Isolated Loading**: Uses `AssemblyLoadContext` for safe, isolated assembly loading
- **Persistent Storage**: Assemblies stored persistently for decompilation access
- **Metadata Extraction**: Complete reflection-based extraction of all public types and members

### Documentation Processing
- **XML Parsing**: Extracts summary, remarks, returns, examples, parameters, and exceptions
- **ECMA-335 ID Mapping**: Generates standardized IDs to link documentation to code elements
- **Multi-Source**: Supports XML documentation, AI-generated content, and decompiled source
- **Null-Safe**: Robust parsing that handles incomplete or malformed documentation

### Search & Navigation
- **Fast Search**: SQLite-backed indexed search across types and members
- **Pagination**: Configurable page size (default 20, max 100) for large assemblies
- **Filtering**: Search by name, namespace, or full qualified name
- **Progressive Disclosure**: Prevents context flooding for AI agents

### Decompilation
- **On-Demand**: Source code generated only when explicitly requested
- **ICSharpCode.Decompiler**: Industry-standard decompiler integration
- **Member & Type Level**: Support for both individual members and entire types
- **Caching Ready**: Infrastructure for caching decompiled code (optional)

### MCP Integration
- **Protocol-Compliant**: Implements Model Context Protocol for AI agent communication
- **Two Tools**: `dll.listTypes` for discovery, `dll.getMemberDetails` for deep dives
- **JSON Responses**: Structured responses with documentation and source code

### Web Interface
- **Single-Page UI**: Clean HTML/JavaScript interface for human interaction
- **Real-Time Search**: Live filtering as you type
- **Upload Interface**: Drag-and-drop support for DLL and XML files
- **Confidence Indicators**: Visual badges showing documentation source (XML/AI/Decompiled)

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                        Web Browser                           │
│                    (HTML/CSS/JavaScript)                     │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/JSON
┌────────────────────────▼────────────────────────────────────┐
│                   ASP.NET Core 9 API                         │
│                   (Minimal API Pattern)                      │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Controllers (Program.cs)                           │   │
│  │  - /api/assemblies/*  - /mcp/*                      │   │
│  └────────────┬──────────────────────┬──────────────────┘   │
│               │                      │                       │
│  ┌────────────▼──────────┐  ┌───────▼───────────────────┐  │
│  │   Services Layer      │  │   Data Access Layer       │  │
│  │  ┌─────────────────┐  │  │  ┌────────────────────┐  │  │
│  │  │ Assembly Loader │  │  │  │ DllMcpDbContext    │  │  │
│  │  │ (Reflection)    │  │  │  │ (EF Core)          │  │  │
│  │  └─────────────────┘  │  │  └────────────────────┘  │  │
│  │  ┌─────────────────┐  │  │  ┌────────────────────┐  │  │
│  │  │ XML Parser      │  │  │  │ AssemblyRepository │  │  │
│  │  │ (ECMA-335)      │  │  │  │ (CRUD operations)  │  │  │
│  │  └─────────────────┘  │  │  └────────────────────┘  │  │
│  │  ┌─────────────────┐  │  │                           │  │
│  │  │ Decompiler      │  │  │                           │  │
│  │  │ (ICSharpCode)   │  │  │                           │  │
│  │  └─────────────────┘  │  │                           │  │
│  └───────────────────────┘  └───────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
                    ┌────▼─────┐
                    │  SQLite  │
                    │ Database │
                    └──────────┘
```

### Data Flow

1. **Upload Flow**
   ```
   User → Upload DLL/XML → AssemblyLoaderService → Reflection API
                                    ↓
                            Extract Metadata
                                    ↓
   XmlDocumentationParser → Map via ECMA-335 IDs
                                    ↓
                         Store in SQLite via EF Core
   ```

2. **Query Flow**
   ```
   Agent/User → MCP Tool or API → AssemblyRepository
                                         ↓
                                  Query SQLite
                                         ↓
                                 Return Results
                                         ↓
                            (Optional) Decompile Source
   ```

## Project Structure

### Root Directory Layout

```
DllMcp/
├── src/                          # Source code
│   └── DllMcp.Api/              # Main API project
├── test/                         # Test projects and scripts
│   ├── TestAssembly/            # Sample assembly for testing
│   ├── integration-test.sh      # Basic integration tests
│   └── integration-test-full.sh # Extended integration tests
├── doccs/                        # Documentation
│   ├── spec.md                  # Technical specification
│   ├── mcp-tools-usage.md       # MCP tools documentation
│   └── known-limitations.md     # Known issues and future work
├── DllMcp.sln                   # Solution file
├── README.md                    # This file
├── IMPLEMENTATION_SUMMARY.md    # Implementation details
└── LICENSE                      # License file
```

### Source Code Structure (`src/DllMcp.Api/`)

#### 1. **Data Layer** (`Data/`)

Responsible for all database operations using Entity Framework Core.

- **`DllMcpDbContext.cs`**
  - Entity Framework Core DbContext
  - Configures entity mappings for Assemblies, Types, and Members
  - Sets up database indexes for performance
  - Handles SQLite-specific configurations
  
- **`AssemblyRepository.cs`**
  - Repository pattern implementation for data access
  - CRUD operations for Assemblies, Types, and Members
  - Search and pagination logic using LINQ
  - Implements upsert patterns with EF Core

**Key Methods:**
```csharp
// Save or update an assembly
Task<string> SaveAssemblyAsync(AssemblyInfo assembly)

// Get types with optional search and pagination
Task<IEnumerable<TypeInfo>> GetTypesAsync(string assemblyId, string? search, int page, int pageSize)

// Get specific member by ID
Task<MemberData?> GetMemberAsync(string memberId)
```

#### 2. **Models** (`Models/`)

Data transfer objects and domain models.

- **`AssemblyInfo.cs`**
  - Represents a loaded .NET assembly
  - Properties: Id, Name, HasXmlDocumentation, AssemblyPath
  - Maps to `Assemblies` table
  
- **`TypeInfo.cs`**
  - Represents a type (class, interface, struct, enum)
  - Properties: Id, AssemblyId, Namespace, Name, FullName, Kind, BaseType, Summary
  - Maps to `Types` table with search indexes
  
- **`MemberData.cs`**
  - Represents a member (method, property, field, event)
  - Contains both XML and AI-generated documentation fields
  - Properties include: Id, TypeId, Name, MemberKind, Signature, XmlSummary, XmlRemarks, etc.
  - Maps to `Members` table
  
- **`MemberDetailResponse.cs`**
  - API response model for member details
  - Combines documentation and optional source code
  - Includes nested types: DocumentationInfo, SourceCodeInfo

#### 3. **Services** (`Services/`)

Business logic and domain services.

- **`AssemblyLoaderService.cs`**
  - Core service for loading and indexing assemblies
  - Creates isolated `AssemblyLoadContext` for each assembly
  - Extracts metadata using .NET Reflection API
  - Coordinates with XmlDocumentationParser
  - Saves extracted data to database
  
  **Key Responsibilities:**
  - Assembly isolation and loading
  - Type and member enumeration
  - Signature generation
  - Metadata storage coordination

- **`Ecma335IdGenerator.cs`**
  - Generates ECMA-335 documentation IDs
  - Maps reflection objects to XML documentation nodes
  - Supports all member types: Types, Methods, Properties, Fields, Events
  
  **ID Format Examples:**
  ```
  T:Namespace.ClassName
  M:Namespace.Class.Method(System.String,System.Int32)
  P:Namespace.Class.PropertyName
  F:Namespace.Class.FieldName
  E:Namespace.Class.EventName
  ```

- **`XmlDocumentationParser.cs`**
  - Parses XML documentation files
  - Extracts: summary, remarks, returns, examples, parameters, exceptions
  - Null-safe element access
  - Returns tuples of documentation elements
  
  **Parsed Elements:**
  - `<summary>` - Brief description
  - `<remarks>` - Detailed remarks
  - `<returns>` - Return value description
  - `<example>` - Usage examples
  - `<param>` - Parameter descriptions (JSON serialized)
  - `<exception>` - Exception documentation (JSON serialized)

- **`DecompilerService.cs`**
  - Wraps ICSharpCode.Decompiler
  - On-demand source code generation
  - Member and type-level decompilation
  - Comprehensive error handling and logging
  
  **Features:**
  - FileNotFoundException and BadImageFormatException handling
  - ILogger integration for diagnostics
  - Configurable decompiler settings

#### 4. **API Endpoints** (`Program.cs`)

Main application entry point with endpoint definitions.

**Structure:**
1. **Service Configuration**
   - CORS setup (development vs production)
   - EF Core DbContext registration
   - Service lifetime configuration (scoped/singleton)
   - Database initialization

2. **API Endpoints**
   - Assembly upload with multipart/form-data
   - Assembly and type listing with pagination
   - Member details with optional decompilation
   - MCP tool endpoints for AI agents

3. **Middleware Pipeline**
   - CORS
   - Static files (wwwroot)
   - Request routing

**Endpoint Categories:**
```csharp
// Assembly Management
POST   /api/assemblies/upload
GET    /api/assemblies
GET    /api/assemblies/{id}/types

// Member Details
GET    /api/members/{id}

// MCP Tools
POST   /mcp/dll.listTypes
POST   /mcp/dll.getMemberDetails
```

#### 5. **Frontend** (`wwwroot/`)

Static HTML/CSS/JavaScript files.

- **`index.html`**
  - Single-page application
  - File upload interface (drag-and-drop support)
  - Assembly browser with real-time search
  - Type listing with filtering
  - Confidence badges (🛡️ Official, ✨ AI, ⚙️ Decompiled)
  - Responsive design with gradient UI

**JavaScript Features:**
- Async/await for API calls
- Real-time search with debouncing
- Dynamic DOM updates
- Error handling and user feedback

#### 6. **Configuration** (`appsettings.json`, `DllMcp.Api.csproj`)

- **`appsettings.json`**
  - Logging configuration
  - Application settings
  - Environment-specific overrides

- **`DllMcp.Api.csproj`**
  - .NET 9 target framework
  - NuGet package references:
    - Microsoft.EntityFrameworkCore.Sqlite (9.0.0)
    - Microsoft.EntityFrameworkCore.Design (9.0.0)
    - ICSharpCode.Decompiler (8.2.0.7535)

## Getting Started

### Prerequisites

- **.NET 9 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- A modern web browser (Chrome, Firefox, Edge, Safari)
- Optional: SQL browser for database inspection (e.g., DB Browser for SQLite)

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/obirler/DllMcp.git
   cd DllMcp
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   cd src/DllMcp.Api
   dotnet run
   ```

5. **Access the application**
   - Open your browser to `http://localhost:5000`
   - Or use the port displayed in the console output

### First Steps

1. **Upload an assembly**
   - Click "Choose File" to select a `.dll` file
   - Optionally select the corresponding `.xml` documentation file
   - Click "Upload Assembly"

2. **Browse types**
   - Click "Browse Types" on an uploaded assembly
   - Use the search box to filter types
   - Click on a type to see its members

3. **Use the API**
   ```bash
   # List assemblies
   curl http://localhost:5000/api/assemblies

   # Upload assembly
   curl -F "dll=@MyLibrary.dll" -F "xml=@MyLibrary.xml" \
        http://localhost:5000/api/assemblies/upload

   # Search types
   curl "http://localhost:5000/api/assemblies/{id}/types?search=Calculator"
   ```

## API Reference

### Assembly Management

#### Upload Assembly
```http
POST /api/assemblies/upload
Content-Type: multipart/form-data

Parameters:
  dll (file, required): The .NET assembly DLL file
  xml (file, optional): The XML documentation file

Response: 
{
  "assemblyId": "guid",
  "message": "Assembly uploaded and indexed successfully"
}
```

#### List Assemblies
```http
GET /api/assemblies

Response:
[
  {
    "id": "guid",
    "name": "AssemblyName",
    "hasXmlDocumentation": true,
    "assemblyPath": "/path/to/assembly.dll"
  }
]
```

#### Get Types
```http
GET /api/assemblies/{assemblyId}/types?search={term}&page={n}&pageSize={size}

Parameters:
  assemblyId (string, required): Assembly GUID
  search (string, optional): Search term for filtering
  page (int, optional, default=1): Page number
  pageSize (int, optional, default=20, max=100): Items per page

Response:
[
  {
    "id": "T:Namespace.ClassName",
    "assemblyId": "guid",
    "namespace": "Namespace",
    "name": "ClassName",
    "fullName": "Namespace.ClassName",
    "kind": "Class",
    "baseType": "System.Object",
    "summary": "Class description"
  }
]
```

### Member Information

#### Get Member Details
```http
GET /api/members/{memberId}?includeSource={bool}

Parameters:
  memberId (string, required): ECMA-335 member ID
  includeSource (bool, optional, default=false): Include decompiled source

Response:
{
  "name": "MethodName",
  "signature": "ReturnType MethodName(ParamType param)",
  "documentation": {
    "source": "xml|ai|none",
    "summary": "Method description",
    "remarks": "Additional details",
    "returns": "Return value description",
    "example": "Usage example code"
  },
  "sourceCode": {
    "available": true,
    "language": "csharp",
    "content": "public void MethodName() { ... }"
  }
}
```

### MCP Tools

#### List Types (MCP)
```http
POST /mcp/dll.listTypes
Content-Type: application/json

{
  "assemblyId": "guid",
  "search": "optional search term",
  "page": 1,
  "pageSize": 20
}

Response:
{
  "types": [ /* array of type objects */ ],
  "page": 1,
  "pageSize": 20
}
```

#### Get Member Details (MCP)
```http
POST /mcp/dll.getMemberDetails
Content-Type: application/json

{
  "memberId": "M:Namespace.Class.Method",
  "includeSource": false
}

Response: (same as GET /api/members/{id})
```

## Database Schema

The system uses SQLite with Entity Framework Core for data persistence.

### Tables

#### Assemblies
```sql
CREATE TABLE Assemblies (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    HasXmlDocumentation INTEGER NOT NULL,
    AssemblyPath TEXT NULL
);
```

#### Types
```sql
CREATE TABLE Types (
    Id TEXT PRIMARY KEY,
    AssemblyId TEXT NOT NULL,
    Namespace TEXT NOT NULL,
    Name TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Kind TEXT NOT NULL,
    BaseType TEXT NULL,
    Summary TEXT NULL,
    FOREIGN KEY (AssemblyId) REFERENCES Assemblies(Id)
);

CREATE INDEX idx_types_search ON Types(FullName);
CREATE INDEX idx_types_namespace ON Types(Namespace);
```

#### Members
```sql
CREATE TABLE Members (
    Id TEXT PRIMARY KEY,
    TypeId TEXT NOT NULL,
    Name TEXT NOT NULL,
    MemberKind TEXT NOT NULL,
    Signature TEXT NOT NULL,
    
    -- XML Documentation
    XmlSummary TEXT NULL,
    XmlRemarks TEXT NULL,
    XmlReturns TEXT NULL,
    XmlExample TEXT NULL,
    XmlParams TEXT NULL,     -- JSON
    XmlExceptions TEXT NULL, -- JSON
    
    -- AI Documentation
    AiSummary TEXT NULL,
    AiExample TEXT NULL,
    
    LastUpdated TEXT NOT NULL,
    FOREIGN KEY (TypeId) REFERENCES Types(Id)
);

CREATE INDEX idx_members_type ON Members(TypeId);
CREATE INDEX idx_members_name ON Members(Name);
```

### Entity Relationships

```
Assemblies (1) ──────── (Many) Types
                              │
                              │
                              └── (Many) Members
```

## Technology Stack

### Backend
- **.NET 9** - Latest .NET framework with improved performance
- **ASP.NET Core Minimal API** - Lightweight, high-performance web API
- **Entity Framework Core 9.0** - Object-relational mapper (ORM)
- **SQLite 3** - Embedded database for metadata storage
- **ICSharpCode.Decompiler 8.2** - IL to C# decompilation

### Frontend
- **HTML5** - Semantic markup
- **CSS3** - Modern styling with gradients and transitions
- **Vanilla JavaScript** - No frameworks, pure DOM manipulation
- **Fetch API** - Async HTTP requests

### Development Tools
- **MSBuild** - Build system
- **dotnet CLI** - Command-line tooling
- **Git** - Version control

### Key Libraries
- `Microsoft.EntityFrameworkCore.Sqlite` - EF Core SQLite provider
- `Microsoft.EntityFrameworkCore.Design` - EF Core design-time tools
- `ICSharpCode.Decompiler` - Decompilation engine
- `System.Reflection` - Metadata extraction
- `System.Xml.Linq` - XML parsing

## Development

### Building from Source

```bash
# Clean previous builds
dotnet clean

# Restore packages
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release
```

### Running in Development Mode

```bash
cd src/DllMcp.Api
dotnet run --environment Development
```

**Development Features:**
- Hot reload enabled
- Detailed error pages
- Permissive CORS (allow any origin)
- Console logging

### Project Configuration

#### Service Lifetimes
- **Scoped**: `DllMcpDbContext`, `AssemblyRepository`, `AssemblyLoaderService`
- **Singleton**: `DecompilerService`

#### CORS Policy
- **Development**: Allow any origin, method, and header
- **Production**: Restrict to specific origins (configure in `appsettings.json`)

### Adding New Features

1. **Add a new endpoint**
   - Edit `Program.cs`
   - Add endpoint mapping after existing endpoints
   - Follow minimal API pattern

2. **Add a new service**
   - Create class in `Services/` folder
   - Register in DI container in `Program.cs`
   - Inject into endpoints or other services

3. **Modify database schema**
   - Update entity models in `Models/`
   - Update `DllMcpDbContext` configurations
   - Delete existing database or use EF Core migrations

## Testing

### Integration Tests

The project includes bash-based integration tests:

```bash
# Basic integration test
./test/integration-test.sh

# Full integration test (includes decompilation)
./test/integration-test-full.sh
```

**Test Coverage:**
- Assembly upload with XML documentation
- Assembly listing
- Type search and pagination
- MCP tool endpoints
- Member details retrieval

### Manual Testing

1. **Build test assembly**
   ```bash
   cd test/TestAssembly
   dotnet build -c Release
   ```

2. **Upload via UI**
   - Navigate to http://localhost:5000
   - Upload `test/TestAssembly/bin/Release/net9.0/TestAssembly.dll`
   - Upload `test/TestAssembly/bin/Release/net9.0/TestAssembly.xml`

3. **Test search**
   - Search for "Calculator"
   - Verify results appear

### API Testing with curl

```bash
# Get assemblies
curl http://localhost:5000/api/assemblies | jq

# Upload assembly
curl -F "dll=@test.dll" -F "xml=@test.xml" \
     http://localhost:5000/api/assemblies/upload | jq

# Search types
curl "http://localhost:5000/api/assemblies/{id}/types?search=Calculator" | jq

# Get member details
curl http://localhost:5000/api/members/M:TestAssembly.Calculator.Add | jq
```

## Troubleshooting

### Common Issues

**Database locked error**
- Close any database browser tools
- Delete `dllmcp.db*` files and restart

**Assembly not loading**
- Ensure DLL targets .NET 9 or is compatible
- Check file permissions
- Verify file is not corrupted

**Search not working**
- Verify database indexes exist
- Check for special characters in search term
- Ensure assembly was fully indexed

**Decompilation fails**
- Check assembly path is stored correctly
- Verify ICSharpCode.Decompiler supports the IL
- Review server logs for detailed errors

## Performance Considerations

- **Upload**: ~1-2 seconds for typical assemblies (<1MB)
- **Search**: <50ms for assemblies with <1000 types
- **Indexing**: ~100-500 types per second
- **Database Size**: ~1-5KB per type, ~500B-2KB per member
- **Decompilation**: 100-500ms per member (uncached)

## Future Enhancements

See `doccs/known-limitations.md` for detailed roadmap:

- **AI Documentation Generation** (Tiers 2-3)
- **Improved Decompilation** (member-level refinements)
- **NuGet Integration** (auto-fetch XML from NuGet.org)
- **Caching** (decompiled code, search results)
- **Full-Text Search** (SQLite FTS5)
- **Authentication** (API keys, OAuth)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

See LICENSE file for details.

## Support

For issues, questions, or contributions:
- Open an issue on GitHub
- Review the specification: `doccs/spec.md`
- Check known limitations: `doccs/known-limitations.md`

---

**Version**: 1.0.0  
**Last Updated**: December 2025  
**.NET Version**: 9.0