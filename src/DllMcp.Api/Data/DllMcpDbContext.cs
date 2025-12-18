using DllMcp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DllMcp.Api.Data;

public class DllMcpDbContext : DbContext
{
    public DllMcpDbContext(DbContextOptions<DllMcpDbContext> options) : base(options)
    {
    }

    public DbSet<AssemblyInfo> Assemblies { get; set; }
    public DbSet<TypeInfo> Types { get; set; }
    public DbSet<MemberData> Members { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssemblyInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<TypeInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssemblyId).IsRequired();
            entity.Property(e => e.Namespace).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.Kind).IsRequired();
            
            entity.HasIndex(e => e.FullName).HasDatabaseName("idx_types_search");
            entity.HasIndex(e => e.Namespace).HasDatabaseName("idx_types_namespace");
        });

        modelBuilder.Entity<MemberData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeId).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.MemberKind).IsRequired();
            entity.Property(e => e.Signature).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired();
            
            entity.HasIndex(e => e.TypeId).HasDatabaseName("idx_members_type");
            entity.HasIndex(e => e.Name).HasDatabaseName("idx_members_name");
        });
    }
}
