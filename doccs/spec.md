# **MCP-DLL Document Proxy Server – Technical Specification**

**Version:** 1.2
**Target Stack:** .NET 9 Minimal API (Backend), Plain HTML/JS (Frontend), SQLite for caching, ILSpy (Decompiler)
**Purpose:** The MCP-DLL Document Proxy Server is a comprehensive "Knowledge Gateway" for .NET assemblies. It bridges the gap between compiled binary libraries and AI agents by providing a unified, queryable interface that scales from high-level documentation to low-level source code.

The system solves the "Black Box" problem using a **Tiered Discovery Strategy**:
1.  **Official Intent:** Serves XML documentation if available.
2.  **Inferred Intent:** Generates AI documentation/examples if XML is missing.
3.  **Ultimate Truth:** Decompiles binary back to C# source code when deep implementation details are required.

By exposing this hybrid metadata via the Model Context Protocol (MCP), the system enables LLMs to navigate large libraries efficiently (via pagination/search) and understand them deeply (via documentation + source code), preventing context flooding while ensuring accuracy.

---

## **1. Project Overview**

The system loads arbitrary .NET assemblies (`.dll`) and their corresponding documentation files (`.xml`) to expose:

*   **Structural Metadata:** Namespaces, Types, Members (via Reflection).
*   **Official Documentation:** Summaries, params, returns, remarks, and examples (via XML parsing).
*   **Synthetic Documentation:** AI-generated explanations and examples (via LLM, only when official docs are missing).
*   **Source Code:** On-demand decompilation of methods/types (via ILSpy).
*   **Smart Navigation:** Search and pagination to handle massive libraries without overwhelming the LLM context window.

---

## **2. System Architecture**

### **2.1 High-Level Components**

1.  **Frontend (static HTML/JS)**
    *   Multi-file uploader (DLL + XML).
    *   Visual "Confidence Indicators" (Shield=Official, Sparkles=AI, Code=Decompiled).
    *   Metadata tree explorer with "View Source" toggle.

2.  **Backend – ASP.NET 9 Minimal API**
    *   **Assembly Loader:** Custom `AssemblyLoadContext` for isolation.
    *   **Metadata Engine:** Reflection + XML Documentation Provider (ECMA-335 ID generation).
    *   **Decompiler Engine:** Wraps `ICSharpCode.Decompiler` to reverse-engineer C# code on demand.
    *   **Hybrid Logic Engine:** Orchestrates the fallback between XML, AI, and Decompilation.
    *   **MCP Endpoints:** Serves JSON to the LLM.

3.  **SQLite Database**
    *   Stores structural metadata (indexed for search).
    *   Stores parsed XML content.
    *   Stores generated AI content.
    *   *Note: Decompiled code is NOT cached in DB due to size; it is generated on-demand.*

4.  **LLM Client**
    *   Invoked only for "Tier 2/3" generation tasks.

---

## **3. Data Flow Overview**

### **Upload → Index → Serve**

1.  **Upload:** User uploads `MyLib.dll` (and optional `MyLib.xml`).
2.  **Indexing:**
    *   Load Assembly.
    *   Extract all Types/Members.
    *   Map XML docs to Members (if XML exists).
    *   Store in SQLite (optimized for text search on Name/Namespace).
3.  **Query (MCP):**
    *   **Discovery:** Agent calls `dll.listTypes(search="Json", limit=20)`.
    *   **Deep Dive:** Agent calls `dll.getMemberDetails(id, includeSource=true)`.
    *   **Response:** Backend combines XML docs + AI Examples + Decompiled Code into one JSON object.

---

## **4. Detailed Backend Specification**

### **4.1 Minimal API Endpoints**

#### **4.1.1 Upload Assembly**
`POST /api/assemblies/upload`
*   **Payload:** `multipart/form-data` (DLLs + XMLs).
*   **Logic:** Pairs files, extracts metadata, populates SQLite.

#### **4.1.2 List Types (Paginated)**
`GET /api/assemblies/{id}/types?search={term}&page={n}&pageSize={m}`
*   **Purpose:** Prevents context flooding.
*   **Logic:** SQL `LIKE` query on Name/Namespace with `LIMIT/OFFSET`.

#### **4.1.3 Get Member Info (Hybrid)**
`GET /api/members/{id}?generateMissing=true&includeSource=true`
*   **Logic:**
    1.  Fetch metadata + XML from DB.
    2.  If `generateMissing=true` & docs missing -> Call LLM.
    3.  If `includeSource=true` -> Call ILSpy Decompiler.
    4.  Return merged result.

### **4.2 MCP Tools**

The MCP server exposes tools designed for "Progressive Disclosure":

*   `dll.listTypes`:
    *   **Args:** `assemblyId`, `search` (optional), `page` (default 1), `pageSize` (default 20).
    *   **Returns:** List of lightweight type summaries.
*   `dll.getMemberDetails`:
    *   **Args:** `memberId`, `includeSource` (boolean, default false).
    *   **Returns:** Full documentation (XML/AI) + optional Source Code.

---

## **5. Metadata Extraction & XML Mapping**

### **5.1 The "Rosetta Stone" (ID Generation)**
Implementation of **ECMA-335** ID generation to link Reflection objects to XML nodes (e.g., `M:Namespace.Class.Method`).

### **5.2 XML Parsing Strategy**
Extracts: `<summary>`, `<remarks>`, `<returns>`, `<example>`, `<exception>`, `<param>`.

### **5.3 Decompilation Strategy (New)**
Uses `ICSharpCode.Decompiler`.
*   **Scope:** Single Member (Method/Property) or Single Type.
*   **Formatting:** Returns standard C# string.
*   **Configuration:** Disable "yield return" or "async/await" sugar if raw logic is preferred (configurable).

---

## **6. SQLite Schema**

### **6.1 Assemblies**
```sql
CREATE TABLE Assemblies (
    Id TEXT PRIMARY KEY,
    Name TEXT,
    HasXmlDocumentation INTEGER
);
```

### **6.2 Types (Indexed)**
```sql
CREATE TABLE Types (
    Id TEXT PRIMARY KEY,
    AssemblyId TEXT,
    Namespace TEXT,
    Name TEXT,
    FullName TEXT, -- Indexed for Search
    Kind TEXT,
    BaseType TEXT,
    Summary TEXT -- Cached from XML
);
CREATE INDEX idx_types_search ON Types(FullName);
```

### **6.3 Members (Consolidated)**
```sql
CREATE TABLE Members (
    Id TEXT PRIMARY KEY,
    TypeId TEXT,
    Name TEXT,
    MemberKind TEXT,
    Signature TEXT,
    
    -- Official Documentation (XML)
    XmlSummary TEXT,
    XmlRemarks TEXT,
    XmlReturns TEXT,
    XmlExample TEXT,
    XmlParams TEXT,     -- JSON
    XmlExceptions TEXT, -- JSON
    
    -- Synthetic Documentation (AI)
    AiSummary TEXT,
    AiExample TEXT,
    
    LastUpdated TEXT
);
```

---

## **7. The "4-Tier" Knowledge Strategy**

The system provides data based on the "Cost vs. Truth" trade-off.

### **Tier 1: Official (Fastest, Free)**
*   **Source:** `.xml` file.
*   **Use Case:** Standard API usage.
*   **Content:** Summary, Params, Remarks.

### **Tier 2: Augmented (Low Cost)**
*   **Source:** LLM (Context: Tier 1).
*   **Use Case:** XML exists but lacks usage examples.
*   **Content:** AI-generated Code Example based on XML specs.

### **Tier 3: Synthetic (High Cost)**
*   **Source:** LLM (Context: Signature only).
*   **Use Case:** No XML available.
*   **Content:** AI-inferred Summary and Example.

### **Tier 4: Source Code (Ultimate Truth)**
*   **Source:** ILSpy Decompiler.
*   **Use Case:** Agent needs to understand internal logic, algorithms, or undocumented side effects.
*   **Content:** Actual C# implementation.
*   **Trigger:** Explicitly requested via `includeSource: true`.

---

## **8. MCP JSON Schema**

Response for `dll.getMemberDetails`:

```json
{
  "name": "CalculateHash",
  "signature": "string CalculateHash(byte[] data)",
  "documentation": {
    "source": "hybrid",
    "summary": "Computes the SHA256 hash.", // XML
    "example": "var hash = crypto.CalculateHash(bytes);" // AI
  },
  "sourceCode": {
    "available": true,
    "language": "csharp",
    "content": "public string CalculateHash(byte[] data) { ... }" // Only if requested
  }
}
```

---

## **9. Frontend Specification**

### **9.1 Features**
*   **Search Bar:** Real-time filtering of types/members (calls `listTypes` with search param).
*   **Source Viewer:** "Show Code" button that calls backend to decompile on the fly.
*   **Confidence Badges:**
    *   🛡️ **Official** (XML)
    *   ✨ **AI Generated**
    *   ⚙️ **Decompiled Source**

---

## **10. Implementation Roadmap**

1.  **Phase 1: Core & DB**
    *   SQLite setup, Assembly Loading, Basic Reflection.
2.  **Phase 2: The XML Engine**
    *   ECMA-335 ID Generator, XML Parser.
3.  **Phase 3: The Decompiler**
    *   Integrate `ICSharpCode.Decompiler`.
    *   Add `includeSource` logic.
4.  **Phase 4: MCP & Search**
    *   Implement Pagination/Search in SQL.
    *   Implement MCP Tools with Tiered Logic.
5.  **Phase 5: UI**
    *   Dashboard with Search and Source View.

---

## **11. Future Expansion**

*   **NuGet Auto-Resolve:** If user uploads `Newtonsoft.Json.dll` but no XML, backend automatically fetches the matching XML from NuGet.org.
*   **Project Context:** Support uploading `.zip` of full solutions (`.sln`) to parse `.cs` files directly instead of decompiling.
