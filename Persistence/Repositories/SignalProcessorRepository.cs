using Microsoft.EntityFrameworkCore;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public class SignalProcessorRepository : ISignalProcessorRepository
{
    private readonly SignalDbContext _db;

    public SignalProcessorRepository(SignalDbContext db)
    {
        _db = db;
    }

    public async Task<SignalProcessor> CreateAsync(SignalProcessor signalProcessor, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(signalProcessor);
        _db.SignalProcessors.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task<SignalProcessor?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _db
            .SignalProcessors
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);

        return entity == null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyCollection<SignalProcessor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.SignalProcessors.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SignalProcessors.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _db.SignalProcessors.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SignalProcessor?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entity = await _db
            .SignalProcessors
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Name == name, cancellationToken);

        return entity == null ? null : ToDomain(entity);
    }

    private static SignalProcessor ToDomain(SignalProcessorEntity sp)
    {
        return SignalProcessor.Restore(
            Guid.Parse(sp.Id),
            sp.Name,
            sp.RecomputeTrigger,
            sp.RecomputeIntervalSec,
            sp.ComputeGraph
        );
    }

    private static SignalProcessorEntity ToEntity(SignalProcessor sp)
    {
        return new SignalProcessorEntity
        {
            Id = sp.Id.ToString(),
            Name = sp.Name,
            RecomputeTrigger = sp.RecomputeTrigger,
            RecomputeIntervalSec = sp.RecomputeIntervalSec,
            ComputeGraph = sp.ComputeGraph,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test@vgt.energy"
        };
    }
}
