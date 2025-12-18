namespace DllMcp.Api.Models;

public class MemberData
{
    public string Id { get; set; } = string.Empty;
    public string TypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MemberKind { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    
    // Official Documentation (XML)
    public string? XmlSummary { get; set; }
    public string? XmlRemarks { get; set; }
    public string? XmlReturns { get; set; }
    public string? XmlExample { get; set; }
    public string? XmlParams { get; set; }
    public string? XmlExceptions { get; set; }
    
    // Synthetic Documentation (AI)
    public string? AiSummary { get; set; }
    public string? AiExample { get; set; }
    
    public DateTime LastUpdated { get; set; }
}
