using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class SignalService : ISignalService
{
    private readonly ISignalRepository _repository;

    public SignalService(ISignalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Signal> CreateAsync(SignalCreateRequest request, CancellationToken ct)
    {
        var signal = new Signal
        {
            Id = request.Id,
            Name = request.Name,
            Input = request.Input!.Value,
            Output = request.Output!.Value,
            Unit = request.Unit,
            DataType = request.DataType!.Value,
            Scope = request.Scope!.Value,
            EdgeInstance = request.EdgeInstance
        };

        return await _repository.CreateAsync(signal, ct);
    }

    public async Task<IReadOnlyCollection<Signal>> GetAllAsync(CancellationToken ct)
    {
        return await _repository.GetAllAsync(ct);
    }

    public async Task<Signal> GetByIdAsync(string id, CancellationToken ct)
    {
        var signal = await _repository.GetByIdAsync(id, ct);
        if (signal == null)
        {
            throw new NotFoundException(nameof(Signal), id);
        }
        
        return signal;
    }
}
