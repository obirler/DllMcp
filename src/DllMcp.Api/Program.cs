using DllMcp.Api.Data;
using DllMcp.Api.Models;
using DllMcp.Api.Services;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Add CORS - configure appropriately for production
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow any origin for testing
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Production: Restrict to specific origins
            // TODO: Configure allowed origins from appsettings.json
            policy.WithOrigins("https://yourdomain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Configure database
var connectionString = "Data Source=dllmcp.db";
builder.Services.AddSingleton(new DatabaseInitializer(connectionString));
builder.Services.AddSingleton(new AssemblyRepository(connectionString));
builder.Services.AddSingleton<AssemblyLoaderService>();
builder.Services.AddSingleton<DecompilerService>();

var app = builder.Build();

// Initialize database
var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
dbInitializer.Initialize();

app.UseCors();
app.UseStaticFiles();

// API Endpoints

// Upload assembly endpoint
app.MapPost("/api/assemblies/upload", async (HttpRequest request, AssemblyLoaderService loaderService) =>
{
    var form = await request.ReadFormAsync();
    var dllFile = form.Files.GetFile("dll");
    var xmlFile = form.Files.GetFile("xml");

    if (dllFile == null)
        return Results.BadRequest("DLL file is required");

    // Save uploaded files temporarily
    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);

    var dllPath = Path.Combine(tempDir, dllFile.FileName);
    using (var stream = File.Create(dllPath))
    {
        await dllFile.CopyToAsync(stream);
    }

    string? xmlPath = null;
    if (xmlFile != null)
    {
        xmlPath = Path.Combine(tempDir, xmlFile.FileName);
        using var stream = File.Create(xmlPath);
        await xmlFile.CopyToAsync(stream);
    }

    try
    {
        var assemblyId = await loaderService.LoadAssemblyAsync(dllPath, xmlPath);
        return Results.Ok(new { assemblyId, message = "Assembly uploaded and indexed successfully" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// List types endpoint
app.MapGet("/api/assemblies/{assemblyId}/types", async (
    string assemblyId,
    AssemblyRepository repository,
    string? search = null,
    int page = 1,
    int pageSize = 20) =>
{
    var types = await repository.GetTypesAsync(assemblyId, search, page, pageSize);
    return Results.Ok(types);
});

// Get member details endpoint
app.MapGet("/api/members/{memberId}", async (
    string memberId,
    AssemblyRepository repository,
    DecompilerService decompiler,
    bool includeSource = false) =>
{
    var member = await repository.GetMemberAsync(memberId);
    if (member == null)
        return Results.NotFound();

    var response = new MemberDetailResponse
    {
        Name = member.Name,
        Signature = member.Signature,
        Documentation = new DocumentationInfo
        {
            Source = !string.IsNullOrEmpty(member.XmlSummary) ? "xml" : 
                     !string.IsNullOrEmpty(member.AiSummary) ? "ai" : "none",
            Summary = member.XmlSummary ?? member.AiSummary,
            Remarks = member.XmlRemarks,
            Returns = member.XmlReturns,
            Example = member.XmlExample ?? member.AiExample
        }
    };

    if (includeSource)
    {
        // Get the assembly info to find the path
        var typeId = member.TypeId;
        var assemblyIdQuery = "SELECT AssemblyId FROM Types WHERE Id = @TypeId";
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        var assemblyId = await connection.QueryFirstOrDefaultAsync<string>(assemblyIdQuery, new { TypeId = typeId });
        
        if (!string.IsNullOrEmpty(assemblyId))
        {
            var assembly = await repository.GetAssemblyAsync(assemblyId);
            if (assembly?.AssemblyPath != null && File.Exists(assembly.AssemblyPath))
            {
                var sourceCode = decompiler.DecompileMember(assembly.AssemblyPath, member.Id);
                response.SourceCode = new SourceCodeInfo
                {
                    Available = sourceCode != null,
                    Language = "csharp",
                    Content = sourceCode
                };
            }
            else
            {
                response.SourceCode = new SourceCodeInfo
                {
                    Available = false,
                    Language = "csharp",
                    Content = null
                };
            }
        }
    }

    return Results.Ok(response);
});

// List assemblies endpoint
app.MapGet("/api/assemblies", async (AssemblyRepository repository) =>
{
    var assemblies = await repository.GetAssembliesAsync();
    return Results.Ok(assemblies);
});

// MCP Tools Endpoints

// dll.listTypes tool
app.MapPost("/mcp/dll.listTypes", async (
    ListTypesRequest request,
    AssemblyRepository repository) =>
{
    var types = await repository.GetTypesAsync(
        request.AssemblyId,
        request.Search,
        request.Page,
        request.PageSize);

    return Results.Ok(new
    {
        types = types.Select(t => new
        {
            t.Id,
            t.Name,
            t.FullName,
            t.Namespace,
            t.Kind,
            t.Summary
        }),
        page = request.Page,
        pageSize = request.PageSize
    });
});

// dll.getMemberDetails tool
app.MapPost("/mcp/dll.getMemberDetails", async (
    GetMemberDetailsRequest request,
    AssemblyRepository repository) =>
{
    var member = await repository.GetMemberAsync(request.MemberId);
    if (member == null)
        return Results.NotFound();

    var response = new MemberDetailResponse
    {
        Name = member.Name,
        Signature = member.Signature,
        Documentation = new DocumentationInfo
        {
            Source = !string.IsNullOrEmpty(member.XmlSummary) ? "xml" :
                     !string.IsNullOrEmpty(member.AiSummary) ? "ai" : "none",
            Summary = member.XmlSummary ?? member.AiSummary,
            Remarks = member.XmlRemarks,
            Returns = member.XmlReturns,
            Example = member.XmlExample ?? member.AiExample
        },
        SourceCode = new SourceCodeInfo
        {
            Available = false,
            Language = "csharp"
        }
    };

    return Results.Ok(response);
});

app.Run();

// Request models
record ListTypesRequest(string AssemblyId, string? Search = null, int Page = 1, int PageSize = 20);
record GetMemberDetailsRequest(string MemberId, bool IncludeSource = false);
