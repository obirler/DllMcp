namespace DllMcp.Api.Models;

public class TypeInfo
{
    public string Id { get; set; } = string.Empty;
    public string AssemblyId { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string? BaseType { get; set; }
    public string? Summary { get; set; }
}
