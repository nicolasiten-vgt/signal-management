using Microsoft.EntityFrameworkCore;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public class CustomFunctionRepository : ICustomFunctionRepository
{
    private readonly SignalDbContext _db;

    public CustomFunctionRepository(SignalDbContext db)
    {
        _db = db;
    }

    public async Task<CustomFunction> CreateAsync(CustomFunction customFunction, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(customFunction);
        _db.CustomFunctions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task<CustomFunction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db
            .CustomFunctions
            .AsNoTracking()
            .FirstOrDefaultAsync(cf => cf.Id == id, cancellationToken);
        
        return entity == null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<CustomFunction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.CustomFunctions.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CustomFunctions.FirstOrDefaultAsync(cf => cf.Id == id, cancellationToken);
        if (entity == null)
        {
            return false;
        }
        
        _db.CustomFunctions.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static CustomFunction ToDomain(CustomFunctionEntity cf)
    {
        return new CustomFunction
        {
            Id = cf.Id,
            Name = cf.Name,
            Language = cf.Language,
            InputParameters = cf.InputParameters,
            OutputParameters = cf.OutputParameters,
            SourceCode = cf.SourceCode,
            Dependencies = cf.Dependencies
        };
    }

    private static CustomFunctionEntity ToEntity(CustomFunction cf)
    {
        return new CustomFunctionEntity
        {
            Id = cf.Id,
            Name = cf.Name,
            Language = cf.Language,
            InputParameters = cf.InputParameters,
            OutputParameters = cf.OutputParameters,
            SourceCode = cf.SourceCode,
            Dependencies = cf.Dependencies
        };
    }
}
