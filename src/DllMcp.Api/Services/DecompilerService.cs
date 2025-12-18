using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System.Reflection.Metadata;

namespace DllMcp.Api.Services;

public class DecompilerService
{
    public string? DecompileMember(string assemblyPath, string memberFullName)
    {
        try
        {
            var decompiler = new CSharpDecompiler(assemblyPath, new DecompilerSettings
            {
                ThrowOnAssemblyResolveErrors = false
            });

            // Try to find and decompile the member
            var typeSystem = decompiler.TypeSystem;
            
            // Parse the member ID to get type and member name
            // Format: M:Namespace.Type.Member or P:Namespace.Type.Member, etc.
            if (memberFullName.Length < 3)
                return null;

            var memberKind = memberFullName[0];
            var fullName = memberFullName.Substring(2); // Remove "M:" or "P:", etc.

            // Extract type name (everything before the last dot)
            var lastDotIndex = fullName.LastIndexOf('.');
            if (lastDotIndex < 0)
                return null;

            var typeName = fullName.Substring(0, lastDotIndex);
            var memberName = fullName.Substring(lastDotIndex + 1);

            // Remove parameter list if present (for methods)
            var parenIndex = memberName.IndexOf('(');
            if (parenIndex > 0)
            {
                memberName = memberName.Substring(0, parenIndex);
            }

            // Find the type
            var type = typeSystem.FindType(new FullTypeName(typeName));
            if (type == null)
                return null;

            // Find the member and decompile
            var members = type.GetMembers(m => m.Name == memberName, GetMemberOptions.IgnoreInheritedMembers);
            var member = members.FirstOrDefault();

            if (member == null)
                return null;

            var code = decompiler.DecompileAsString(member.MetadataToken);
            return code;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public string? DecompileType(string assemblyPath, string typeFullName)
    {
        try
        {
            var decompiler = new CSharpDecompiler(assemblyPath, new DecompilerSettings
            {
                ThrowOnAssemblyResolveErrors = false
            });

            // Remove "T:" prefix if present
            var typeName = typeFullName.StartsWith("T:") ? typeFullName.Substring(2) : typeFullName;

            var typeSystem = decompiler.TypeSystem;
            var type = typeSystem.FindType(new FullTypeName(typeName));

            if (type == null)
                return null;

            var code = decompiler.DecompileTypeAsString(new FullTypeName(typeName));
            return code;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
