using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence;

public class SignalDbContext : DbContext
{
    public SignalDbContext(DbContextOptions<SignalDbContext> options) : base(options)
    {
    }

    public DbSet<SignalEntity> Signals => Set<SignalEntity>();
    public DbSet<CustomFunctionEntity> CustomFunctions => Set<CustomFunctionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SignalEntity>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).IsRequired();
            entity.Property(s => s.Name).IsRequired();
            entity.Property(s => s.Input).IsRequired();
            entity.Property(s => s.Output).IsRequired();
            entity.Property(s => s.Unit).IsRequired();
            entity.Property(s => s.DataType).IsRequired();
            entity.Property(s => s.Scope).IsRequired();
            entity.Property(s => s.EdgeInstance);
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.Property(s => s.CreatedBy).IsRequired();
        });

        modelBuilder.Entity<CustomFunctionEntity>(entity =>
        {
            entity.HasKey(cf => cf.Id);
            entity.Property(cf => cf.Id).IsRequired();
            entity.Property(cf => cf.Name).IsRequired();
            entity.Property(cf => cf.Language).IsRequired();
            entity.Property(cf => cf.SourceCode).IsRequired();
            entity.Property(cf => cf.Dependencies);
            entity.Property(cf => cf.InputParameters)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ParameterDefinition>>(v, (JsonSerializerOptions?)null));
            entity.Property(cf => cf.OutputParameters)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ParameterDefinition>>(v, (JsonSerializerOptions?)null));
        });
    }
}

public class SignalDbContextFactory : IDesignTimeDbContextFactory<SignalDbContext>
{
    public SignalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SignalDbContext>();
        optionsBuilder.UseNpgsql();

        return new SignalDbContext(optionsBuilder.Options);
    }
}