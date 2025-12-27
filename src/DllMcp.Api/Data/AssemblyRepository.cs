using DllMcp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DllMcp.Api.Data;

public class AssemblyRepository
{
    private readonly DllMcpDbContext _context;

    public AssemblyRepository(DllMcpDbContext context)
    {
        _context = context;
    }

    public async Task<string> SaveAssemblyAsync(AssemblyInfo assembly)
    {
        var existing = await _context.Assemblies.FindAsync(assembly.Id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(assembly);
        }
        else
        {
            await _context.Assemblies.AddAsync(assembly);
        }
        await _context.SaveChangesAsync();
        return assembly.Id;
    }

    public async Task<IEnumerable<TypeInfo>> GetTypesAsync(
        string assemblyId,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;

        var query = _context.Types
            .Where(t => t.AssemblyId == assemblyId);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => 
                EF.Functions.Like(t.FullName, $"%{search}%") ||
                EF.Functions.Like(t.Name, $"%{search}%") ||
                EF.Functions.Like(t.Namespace, $"%{search}%"));
        }

        return await query
            .OrderBy(t => t.FullName)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task SaveTypeAsync(TypeInfo type)
    {
        var existing = await _context.Types.FindAsync(type.Id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(type);
        }
        else
        {
            await _context.Types.AddAsync(type);
        }
        await _context.SaveChangesAsync();
    }

    public async Task SaveMemberAsync(MemberData member)
    {
        var existing = await _context.Members.FindAsync(member.Id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(member);
        }
        else
        {
            await _context.Members.AddAsync(member);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<MemberData?> GetMemberAsync(string memberId)
    {
        return await _context.Members.FindAsync(memberId);
    }

    public async Task<AssemblyInfo?> GetAssemblyAsync(string assemblyId)
    {
        return await _context.Assemblies.FindAsync(assemblyId);
    }

    public async Task<IEnumerable<AssemblyInfo>> GetAssembliesAsync()
    {
        return await _context.Assemblies.ToListAsync();
    }
}
