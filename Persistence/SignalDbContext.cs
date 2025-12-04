using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence;

public class SignalDbContext : DbContext
{
    public SignalDbContext(DbContextOptions<SignalDbContext> options) : base(options)
    {
    }

    public DbSet<SignalEntity> Signals => Set<SignalEntity>();

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
    }
}

public class BloggingContextFactory : IDesignTimeDbContextFactory<SignalDbContext>
{
    public SignalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SignalDbContext>();
        optionsBuilder.UseNpgsql();

        return new SignalDbContext(optionsBuilder.Options);
    }
}