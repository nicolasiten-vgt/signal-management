using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public interface ISimpleOperationTypeRepository
{
    Task<IReadOnlyCollection<SignalProcessorOperationType>> GetAllAsync();
}
