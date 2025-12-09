using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public interface ISignalProcessorService
{
    Task<SignalProcessor> CreateAsync(SignalProcessorCreateRequest request, CancellationToken ct);
    Task<IReadOnlyCollection<SignalProcessor>> GetAllAsync(CancellationToken ct);
    Task<SignalProcessor> GetByIdAsync(string id, CancellationToken ct);
    Task DeleteAsync(string id, CancellationToken ct);
    Task<SignalProcessorExecutionResult> InvokeAsync(string id, IDictionary<string, string> signalInputs, CancellationToken ct);
}
