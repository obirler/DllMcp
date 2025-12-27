# Known Limitations and Future Improvements

## Current Implementation Status

### ✅ Implemented Features
- Assembly upload and indexing
- XML documentation parsing and mapping
- Type and member metadata extraction
- Search and pagination
- MCP Tools (dll.listTypes, dll.getMemberDetails)
- Web interface with search
- Database persistence
- Assembly path storage

### ⚠️ Partial Implementation

#### Decompilation (Tier 4)
The decompilation infrastructure is in place but currently has limitations:
- **Issue**: The ICSharpCode.Decompiler integration can locate types but has difficulty mapping ECMA-335 member IDs to decompiler metadata tokens
- **Workaround**: Type-level decompilation works better than member-level
- **Future Fix**: Need to improve the member ID parsing or use alternative decompilation strategies

### 🚧 Not Yet Implemented

#### AI Documentation Generation (Tiers 2 & 3)
- Tier 2: AI-augmented documentation (AI-generated examples when XML exists but lacks examples)
- Tier 3: AI-synthetic documentation (AI-inferred summaries when no XML exists)
- **Reason**: Requires LLM client integration (OpenAI, Azure OpenAI, etc.)
- **Priority**: Medium - the XML documentation path works well for documented assemblies

## Future Enhancements

1. **Complete Decompilation Support**
   - Improve ECMA-335 ID to metadata token mapping
   - Add support for decompiling nested types
   - Add support for generic methods

2. **LLM Integration**
   - Add configurable LLM client (OpenAI, Azure OpenAI, Anthropic)
   - Implement Tier 2 augmentation (generate examples from XML)
   - Implement Tier 3 synthesis (generate docs from signatures)

3. **NuGet Integration**
   - Auto-fetch XML documentation from NuGet.org when missing
   - Support NuGet package resolution for dependencies

4. **Enhanced UI**
   - Member detail view with tabs (Documentation/Source/Examples)
   - Syntax highlighting for decompiled code
   - Diff view for comparing versions

5. **Performance Optimizations**
   - Cache decompiled code in database
   - Add full-text search using SQLite FTS5
   - Implement incremental indexing for large assemblies

6. **Security Enhancements**
   - Add authentication and authorization
   - Implement rate limiting for API endpoints
   - Add virus scanning for uploaded assemblies

## Workarounds for Current Limitations

### For Decompilation
Until member-level decompilation is fixed, you can:
1. Use type-level decompilation to see the entire class
2. View XML documentation which often contains the most important information
3. Use a separate decompiler tool like ILSpy for complex cases

### For AI Documentation
Until LLM integration is added:
1. Ensure your assemblies have XML documentation files
2. Use descriptive member names that are self-documenting
3. Add XML documentation to your own assemblies before uploading
