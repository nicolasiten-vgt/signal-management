using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public interface ICustomFunctionRepository
{
    Task<CustomFunction> CreateAsync(CustomFunction customFunction, CancellationToken cancellationToken = default);

    Task<CustomFunction?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    Task<CustomFunction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyCollection<CustomFunction>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
