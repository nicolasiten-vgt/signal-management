using Microsoft.EntityFrameworkCore;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public class SignalRepository : ISignalRepository
{
    private readonly SignalDbContext _db;

    public SignalRepository(SignalDbContext db)
    {
        _db = db;
    }

    public async Task<Signal> CreateAsync(Signal signal, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(signal);
        _db.Signals.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task<Signal?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _db
            .Signals
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        
        return entity == null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<Signal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.Signals.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(ToDomain).ToList();
    }

    private static Signal ToDomain(SignalEntity s)
    {
        return new Signal
        {
            Id = s.Id,
            Name = s.Name,
            Input = s.Input,
            Output = s.Output,
            Unit = s.Unit,
            DataType = s.DataType,
            Scope = s.Scope,
            EdgeInstance = s.EdgeInstance,
            CreatedAt = s.CreatedAt,
            CreatedBy = s.CreatedBy
        };
    }

    private static SignalEntity ToEntity(Signal s)
    {
        return new SignalEntity
        {
            Id = s.Id,
            Name = s.Name,
            Input = s.Input,
            Output = s.Output,
            Unit = s.Unit,
            DataType = s.DataType,
            Scope = s.Scope,
            EdgeInstance = s.EdgeInstance,
            CreatedAt = s.CreatedAt,
            CreatedBy = s.CreatedBy
        };
    }
}