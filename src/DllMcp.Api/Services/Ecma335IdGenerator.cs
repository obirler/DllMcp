using System.Reflection;
using System.Text;

namespace DllMcp.Api.Services;

/// <summary>
/// Generates ECMA-335 documentation IDs for linking Reflection objects to XML documentation.
/// Examples: T:Namespace.ClassName, M:Namespace.Class.Method(System.String), P:Namespace.Class.Property
/// </summary>
public static class Ecma335IdGenerator
{
    public static string GetId(Type type)
    {
        return "T:" + GetTypeName(type);
    }

    public static string GetId(MethodInfo method)
    {
        var sb = new StringBuilder();
        sb.Append("M:");
        sb.Append(GetTypeName(method.DeclaringType!));
        sb.Append('.');
        sb.Append(method.Name);

        var parameters = method.GetParameters();
        if (parameters.Length > 0)
        {
            sb.Append('(');
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(GetTypeName(parameters[i].ParameterType));
            }
            sb.Append(')');
        }

        return sb.ToString();
    }

    public static string GetId(PropertyInfo property)
    {
        var sb = new StringBuilder();
        sb.Append("P:");
        sb.Append(GetTypeName(property.DeclaringType!));
        sb.Append('.');
        sb.Append(property.Name);

        return sb.ToString();
    }

    public static string GetId(FieldInfo field)
    {
        var sb = new StringBuilder();
        sb.Append("F:");
        sb.Append(GetTypeName(field.DeclaringType!));
        sb.Append('.');
        sb.Append(field.Name);

        return sb.ToString();
    }

    public static string GetId(EventInfo eventInfo)
    {
        var sb = new StringBuilder();
        sb.Append("E:");
        sb.Append(GetTypeName(eventInfo.DeclaringType!));
        sb.Append('.');
        sb.Append(eventInfo.Name);

        return sb.ToString();
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var sb = new StringBuilder();
            var genericTypeName = type.GetGenericTypeDefinition().FullName!;
            var backtickIndex = genericTypeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                genericTypeName = genericTypeName.Substring(0, backtickIndex);
            }
            
            sb.Append(genericTypeName);
            sb.Append('{');
            
            var genericArgs = type.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(GetTypeName(genericArgs[i]));
            }
            
            sb.Append('}');
            return sb.ToString();
        }

        return type.FullName ?? type.Name;
    }
}
