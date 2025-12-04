using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public interface ICustomFunctionService
{
    Task<CustomFunction> CreateAsync(CustomFunctionCreateRequest request, CancellationToken ct);
    Task<IReadOnlyCollection<CustomFunction>> GetAllAsync(CancellationToken ct);
    Task<CustomFunction> GetByIdAsync(Guid id, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
