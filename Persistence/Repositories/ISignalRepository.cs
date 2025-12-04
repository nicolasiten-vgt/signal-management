using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public interface ISignalRepository
{
    Task<Signal> CreateAsync(Signal signal, CancellationToken cancellationToken = default);
    
    Task<Signal?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyCollection<Signal>> GetAllAsync(CancellationToken cancellationToken = default);
}