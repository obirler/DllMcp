using Dapper;
using DllMcp.Api.Models;
using Microsoft.Data.Sqlite;

namespace DllMcp.Api.Data;

public class AssemblyRepository
{
    private readonly string _connectionString;

    public AssemblyRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string> SaveAssemblyAsync(AssemblyInfo assembly)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT OR REPLACE INTO Assemblies (Id, Name, HasXmlDocumentation, AssemblyPath) 
              VALUES (@Id, @Name, @HasXmlDocumentation, @AssemblyPath)",
            assembly);
        return assembly.Id;
    }

    public async Task<IEnumerable<TypeInfo>> GetTypesAsync(
        string assemblyId,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        using var connection = new SqliteConnection(_connectionString);
        var offset = (page - 1) * pageSize;

        var sql = @"
            SELECT * FROM Types 
            WHERE AssemblyId = @AssemblyId";

        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (FullName LIKE @Search OR Name LIKE @Search OR Namespace LIKE @Search)";
        }

        sql += " ORDER BY FullName LIMIT @PageSize OFFSET @Offset";

        return await connection.QueryAsync<TypeInfo>(sql, new
        {
            AssemblyId = assemblyId,
            Search = $"%{search}%",
            PageSize = pageSize,
            Offset = offset
        });
    }

    public async Task SaveTypeAsync(TypeInfo type)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT OR REPLACE INTO Types 
              (Id, AssemblyId, Namespace, Name, FullName, Kind, BaseType, Summary) 
              VALUES (@Id, @AssemblyId, @Namespace, @Name, @FullName, @Kind, @BaseType, @Summary)",
            type);
    }

    public async Task SaveMemberAsync(MemberData member)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT OR REPLACE INTO Members 
              (Id, TypeId, Name, MemberKind, Signature, 
               XmlSummary, XmlRemarks, XmlReturns, XmlExample, XmlParams, XmlExceptions,
               AiSummary, AiExample, LastUpdated) 
              VALUES 
              (@Id, @TypeId, @Name, @MemberKind, @Signature,
               @XmlSummary, @XmlRemarks, @XmlReturns, @XmlExample, @XmlParams, @XmlExceptions,
               @AiSummary, @AiExample, @LastUpdated)",
            member);
    }

    public async Task<MemberData?> GetMemberAsync(string memberId)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<MemberData>(
            "SELECT * FROM Members WHERE Id = @Id",
            new { Id = memberId });
    }

    public async Task<AssemblyInfo?> GetAssemblyAsync(string assemblyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<AssemblyInfo>(
            "SELECT * FROM Assemblies WHERE Id = @Id",
            new { Id = assemblyId });
    }

    public async Task<IEnumerable<AssemblyInfo>> GetAssembliesAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<AssemblyInfo>("SELECT * FROM Assemblies");
    }
}
