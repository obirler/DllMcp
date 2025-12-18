# MCP-DLL Document Proxy Server

A comprehensive "Knowledge Gateway" for .NET assemblies that bridges the gap between compiled binary libraries and AI agents. This system provides a unified, queryable interface that scales from high-level documentation to low-level source code.

## Features

- **Assembly Loading**: Upload and load .NET assemblies (`.dll`) with their XML documentation files
- **Metadata Extraction**: Extract namespaces, types, and members using Reflection
- **XML Documentation Parsing**: Parse and map XML documentation to code members using ECMA-335 ID generation
- **Source Code Decompilation**: On-demand decompilation using ICSharpCode.Decompiler
- **Smart Navigation**: Search and pagination support for handling large libraries
- **MCP Tools**: Model Context Protocol endpoints for AI agent integration
- **Web Interface**: Interactive HTML/JavaScript frontend for browsing assemblies

## Architecture

### Backend (ASP.NET Core 9 Minimal API)
- **Assembly Loader**: Custom `AssemblyLoadContext` for isolated assembly loading
- **Metadata Engine**: Reflection-based metadata extraction with ECMA-335 ID generation
- **Decompiler Engine**: Wraps `ICSharpCode.Decompiler` for on-demand C# source code generation
- **SQLite Database**: Stores structural metadata, types, members, and documentation

### Frontend (HTML/JavaScript)
- Multi-file uploader (DLL + XML)
- Assembly browser with search functionality
- Type explorer with documentation display
- Confidence indicators (Official XML, AI Generated, Decompiled Source)

## Getting Started

### Prerequisites
- .NET 9 SDK or later

### Running the Application

1. Build the project:
```bash
dotnet build
```

2. Run the application:
```bash
cd src/DllMcp.Api
dotnet run
```

3. Open your browser and navigate to `http://localhost:5000` (or the port shown in the console)

## API Endpoints

### Assembly Management
- `POST /api/assemblies/upload` - Upload DLL and optional XML documentation
- `GET /api/assemblies` - List all loaded assemblies
- `GET /api/assemblies/{id}/types` - Get types from an assembly (with search and pagination)

### Member Information
- `GET /api/members/{id}` - Get detailed member information
  - Query parameters:
    - `includeSource=true` - Include decompiled source code

### MCP Tools
- `POST /mcp/dll.listTypes` - List types for MCP agents
- `POST /mcp/dll.getMemberDetails` - Get member details for MCP agents

## Database Schema

The system uses SQLite with the following schema:

### Assemblies
Stores loaded assembly information and whether XML documentation is available.

### Types
Stores type information (classes, interfaces, structs, enums) with full-text search indexing on type names and namespaces.

### Members
Stores member information (methods, properties, fields, events) with both XML and AI-generated documentation fields.

## 4-Tier Knowledge Strategy

1. **Tier 1: Official (XML)** - Direct XML documentation from the assembly
2. **Tier 2: Augmented (AI + XML)** - AI-generated examples based on XML specs
3. **Tier 3: Synthetic (AI only)** - AI-inferred documentation when XML is missing
4. **Tier 4: Source Code** - Decompiled C# implementation for deep understanding

## Technology Stack

- .NET 9 (ASP.NET Core Minimal API)
- SQLite (via Entity Framework Core)
- Entity Framework Core 9.0 (ORM)
- ICSharpCode.Decompiler (C# decompilation)
- Plain HTML/CSS/JavaScript (frontend)

## Project Structure

```
DllMcp/
├── src/
│   └── DllMcp.Api/
│       ├── Data/              # Database initialization and repositories
│       ├── Models/            # Data models
│       ├── Services/          # Core services (loader, decompiler, XML parser)
│       ├── wwwroot/           # Static frontend files
│       └── Program.cs         # Application entry point and API endpoints
├── doccs/
│   └── spec.md               # Detailed technical specification
└── README.md
```

## License

See LICENSE file for details.