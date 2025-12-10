using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public class SimpleOperationTypeRepository : ISimpleOperationTypeRepository
{
    public Task<IReadOnlyCollection<ISignalProcessorSimpleOperation>> GetAllAsync()
    {
        return Task.FromResult(SignalProcessorSimpleOperations.All);
    }
}
