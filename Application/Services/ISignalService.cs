using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public interface ISignalService
{
    Task<Signal> CreateAsync(SignalCreateRequest request, CancellationToken ct);
    Task<IReadOnlyList<Signal>> GetAllAsync(CancellationToken ct);
    Task<Signal> GetByIdAsync(string id, CancellationToken ct);
}