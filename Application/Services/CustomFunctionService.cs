using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class CustomFunctionService : ICustomFunctionService
{
    private readonly ICustomFunctionRepository _repository;

    public CustomFunctionService(ICustomFunctionRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomFunction> CreateAsync(CustomFunctionCreateRequest request, CancellationToken ct)
    {
        var customFunction = new CustomFunction
        {
            Name = request.Name,
            Language = request.Language!.Value,
            InputParameters = request.InputParameters,
            OutputParameters = request.OutputParameters,
            SourceCode = request.SourceCode,
            Dependencies = request.Dependencies
        };

        return await _repository.CreateAsync(customFunction, ct);
    }

    public async Task<IReadOnlyList<CustomFunction>> GetAllAsync(CancellationToken ct)
    {
        return await _repository.GetAllAsync(ct);
    }

    public async Task<CustomFunction> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var customFunction = await _repository.GetByIdAsync(id, ct);
        if (customFunction == null)
        {
            throw new NotFoundException(nameof(CustomFunction), id);
        }
        
        return customFunction;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        if (!deleted)
        {
            throw new NotFoundException(nameof(CustomFunction), id);
        }
    }
}
