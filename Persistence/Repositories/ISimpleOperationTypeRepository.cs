using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public interface ISimpleOperationTypeRepository
{
    Task<IReadOnlyCollection<ISignalProcessorSimpleOperation>> GetAllAsync();
}
