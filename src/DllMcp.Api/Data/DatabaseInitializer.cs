using Microsoft.Data.Sqlite;

namespace DllMcp.Api.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Assemblies (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                HasXmlDocumentation INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Types (
                Id TEXT PRIMARY KEY,
                AssemblyId TEXT NOT NULL,
                Namespace TEXT NOT NULL,
                Name TEXT NOT NULL,
                FullName TEXT NOT NULL,
                Kind TEXT NOT NULL,
                BaseType TEXT,
                Summary TEXT,
                FOREIGN KEY (AssemblyId) REFERENCES Assemblies(Id)
            );

            CREATE INDEX IF NOT EXISTS idx_types_search ON Types(FullName);
            CREATE INDEX IF NOT EXISTS idx_types_namespace ON Types(Namespace);

            CREATE TABLE IF NOT EXISTS Members (
                Id TEXT PRIMARY KEY,
                TypeId TEXT NOT NULL,
                Name TEXT NOT NULL,
                MemberKind TEXT NOT NULL,
                Signature TEXT NOT NULL,
                
                XmlSummary TEXT,
                XmlRemarks TEXT,
                XmlReturns TEXT,
                XmlExample TEXT,
                XmlParams TEXT,
                XmlExceptions TEXT,
                
                AiSummary TEXT,
                AiExample TEXT,
                
                LastUpdated TEXT NOT NULL,
                FOREIGN KEY (TypeId) REFERENCES Types(Id)
            );

            CREATE INDEX IF NOT EXISTS idx_members_type ON Members(TypeId);
            CREATE INDEX IF NOT EXISTS idx_members_name ON Members(Name);
        ";

        command.ExecuteNonQuery();
    }
}
