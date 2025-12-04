using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public interface ISignalProcessorRepository
{
    Task<SignalProcessor> CreateAsync(SignalProcessor signalProcessor, CancellationToken cancellationToken = default);

    Task<SignalProcessor?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SignalProcessor>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<SignalProcessor?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
