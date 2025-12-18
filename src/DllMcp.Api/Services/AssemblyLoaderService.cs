using System.Reflection;
using System.Runtime.Loader;
using DllMcp.Api.Data;
using DllMcp.Api.Models;

namespace DllMcp.Api.Services;

public class AssemblyLoaderService
{
    private readonly AssemblyRepository _repository;
    private readonly Dictionary<string, AssemblyLoadContext> _loadContexts = new();

    public AssemblyLoaderService(AssemblyRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> LoadAssemblyAsync(string dllPath, string? xmlPath = null)
    {
        // Create isolated load context
        var contextName = $"AssemblyContext_{Guid.NewGuid()}";
        var loadContext = new AssemblyLoadContext(contextName, isCollectible: true);

        // Load assembly
        var assembly = loadContext.LoadFromAssemblyPath(dllPath);
        var assemblyId = Guid.NewGuid().ToString();

        // Parse XML documentation if provided
        XmlDocumentationParser? xmlParser = null;
        bool hasXml = false;
        if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
        {
            xmlParser = new XmlDocumentationParser();
            xmlParser.LoadXmlDocumentation(xmlPath);
            hasXml = true;
        }

        // Save assembly info
        var assemblyInfo = new AssemblyInfo
        {
            Id = assemblyId,
            Name = assembly.GetName().Name ?? "Unknown",
            HasXmlDocumentation = hasXml
        };
        await _repository.SaveAssemblyAsync(assemblyInfo);

        // Extract types
        var types = assembly.GetExportedTypes();
        foreach (var type in types)
        {
            await IndexTypeAsync(type, assemblyId, xmlParser);
        }

        _loadContexts[assemblyId] = loadContext;
        return assemblyId;
    }

    private async Task IndexTypeAsync(Type type, string assemblyId, XmlDocumentationParser? xmlParser)
    {
        var typeId = Ecma335IdGenerator.GetId(type);
        
        // Get XML documentation for type
        string? summary = null;
        if (xmlParser != null)
        {
            var (xmlSummary, _, _, _, _, _) = xmlParser.GetDocumentation(typeId);
            summary = xmlSummary;
        }

        var typeInfo = new Models.TypeInfo
        {
            Id = typeId,
            AssemblyId = assemblyId,
            Namespace = type.Namespace ?? string.Empty,
            Name = type.Name,
            FullName = type.FullName ?? type.Name,
            Kind = GetTypeKind(type),
            BaseType = type.BaseType?.FullName,
            Summary = summary
        };

        await _repository.SaveTypeAsync(typeInfo);

        // Index methods
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            if (method.IsSpecialName) continue; // Skip property getters/setters
            await IndexMemberAsync(method, typeId, xmlParser);
        }

        // Index properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var property in properties)
        {
            await IndexMemberAsync(property, typeId, xmlParser);
        }

        // Index fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var field in fields)
        {
            await IndexMemberAsync(field, typeId, xmlParser);
        }

        // Index events
        var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var eventInfo in events)
        {
            await IndexMemberAsync(eventInfo, typeId, xmlParser);
        }
    }

    private async Task IndexMemberAsync(System.Reflection.MemberInfo memberInfo, string typeId, XmlDocumentationParser? xmlParser)
    {
        string memberId = memberInfo switch
        {
            MethodInfo method => Ecma335IdGenerator.GetId(method),
            PropertyInfo property => Ecma335IdGenerator.GetId(property),
            FieldInfo field => Ecma335IdGenerator.GetId(field),
            EventInfo eventInfo => Ecma335IdGenerator.GetId(eventInfo),
            _ => throw new NotSupportedException($"Member type {memberInfo.GetType().Name} not supported")
        };

        // Get XML documentation
        string? xmlSummary = null, xmlRemarks = null, xmlReturns = null, xmlExample = null, xmlParams = null, xmlExceptions = null;
        if (xmlParser != null)
        {
            (xmlSummary, xmlRemarks, xmlReturns, xmlExample, xmlParams, xmlExceptions) = xmlParser.GetDocumentation(memberId);
        }

        var member = new MemberData
        {
            Id = memberId,
            TypeId = typeId,
            Name = memberInfo.Name,
            MemberKind = GetMemberKind(memberInfo),
            Signature = GetSignature(memberInfo),
            XmlSummary = xmlSummary,
            XmlRemarks = xmlRemarks,
            XmlReturns = xmlReturns,
            XmlExample = xmlExample,
            XmlParams = xmlParams,
            XmlExceptions = xmlExceptions,
            LastUpdated = DateTime.UtcNow
        };

        await _repository.SaveMemberAsync(member);
    }

    private static string GetTypeKind(Type type)
    {
        if (type.IsInterface) return "Interface";
        if (type.IsEnum) return "Enum";
        if (type.IsValueType) return "Struct";
        if (type.IsClass && type.IsAbstract && type.IsSealed) return "Static Class";
        if (type.IsClass) return "Class";
        return "Type";
    }

    private static string GetMemberKind(System.Reflection.MemberInfo member)
    {
        return member.MemberType switch
        {
            MemberTypes.Method => "Method",
            MemberTypes.Property => "Property",
            MemberTypes.Field => "Field",
            MemberTypes.Event => "Event",
            _ => "Member"
        };
    }

    private static string GetSignature(System.Reflection.MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            MethodInfo method => $"{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})",
            PropertyInfo property => $"{property.PropertyType.Name} {property.Name}",
            FieldInfo field => $"{field.FieldType.Name} {field.Name}",
            EventInfo eventInfo => $"event {eventInfo.EventHandlerType?.Name} {eventInfo.Name}",
            _ => memberInfo.Name
        };
    }
}
