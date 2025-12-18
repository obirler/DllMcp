using System.Xml.Linq;

namespace DllMcp.Api.Services;

public class XmlDocumentationParser
{
    private readonly Dictionary<string, XElement> _documentation = new();

    public void LoadXmlDocumentation(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            return;

        var doc = XDocument.Load(xmlPath);
        var members = doc.Root?.Element("members")?.Elements("member");

        if (members == null)
            return;

        foreach (var member in members)
        {
            var name = member.Attribute("name")?.Value;
            if (name != null)
            {
                _documentation[name] = member;
            }
        }
    }

    public (string? summary, string? remarks, string? returns, string? example, string? parameters, string? exceptions) GetDocumentation(string memberId)
    {
        if (!_documentation.TryGetValue(memberId, out var element))
            return (null, null, null, null, null, null);

        var summary = element.Element("summary")?.Value.Trim();
        var remarks = element.Element("remarks")?.Value.Trim();
        var returns = element.Element("returns")?.Value.Trim();
        var example = element.Element("example")?.Value.Trim();

        // Collect parameters
        var parameters = element.Elements("param")
            .Select(p => new { Name = p.Attribute("name")?.Value, Description = p.Value.Trim() })
            .Where(p => p.Name != null)
            .ToDictionary(p => p.Name!, p => p.Description);
        var parametersJson = parameters.Any() 
            ? System.Text.Json.JsonSerializer.Serialize(parameters) 
            : null;

        // Collect exceptions
        var exceptions = element.Elements("exception")
            .Select(e => new { Type = e.Attribute("cref")?.Value, Description = e.Value.Trim() })
            .Where(e => e.Type != null)
            .ToDictionary(e => e.Type!, e => e.Description);
        var exceptionsJson = exceptions.Any() 
            ? System.Text.Json.JsonSerializer.Serialize(exceptions) 
            : null;

        return (summary, remarks, returns, example, parametersJson, exceptionsJson);
    }

    public bool HasDocumentation(string memberId)
    {
        return _documentation.ContainsKey(memberId);
    }
}
