using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public interface ISignalProcessorOperationTypeService
{
    Task<IReadOnlyCollection<SignalProcessorOperationType>> GetAllAsync(CancellationToken ct = default);
}
