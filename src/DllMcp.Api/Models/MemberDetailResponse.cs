namespace DllMcp.Api.Models;

public class MemberDetailResponse
{
    public string Name { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public DocumentationInfo Documentation { get; set; } = new();
    public SourceCodeInfo? SourceCode { get; set; }
}

public class DocumentationInfo
{
    public string Source { get; set; } = string.Empty; // "xml", "ai", "hybrid"
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? Returns { get; set; }
    public string? Example { get; set; }
}

public class SourceCodeInfo
{
    public bool Available { get; set; }
    public string Language { get; set; } = "csharp";
    public string? Content { get; set; }
}
