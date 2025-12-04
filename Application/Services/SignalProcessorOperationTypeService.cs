using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class SignalProcessorOperationTypeService : ISignalProcessorOperationTypeService
{
    private readonly ISimpleOperationTypeRepository _simpleOperationTypeRepository;
    private readonly ICustomFunctionService _customFunctionService;

    public SignalProcessorOperationTypeService(
        ISimpleOperationTypeRepository simpleOperationTypeRepository,
        ICustomFunctionService customFunctionService)
    {
        _simpleOperationTypeRepository = simpleOperationTypeRepository;
        _customFunctionService = customFunctionService;
    }

    public async Task<List<SignalProcessorOperationType>> GetAllAsync(CancellationToken ct = default)
    {
        List<SignalProcessorOperationType> result = new();

        // Get simple operations
        List<SignalProcessorOperationType> simpleOperations = await _simpleOperationTypeRepository.GetAllAsync();
        result.AddRange(simpleOperations);

        // Get custom functions and map them to operation types
        IReadOnlyList<CustomFunction> customFunctions = await _customFunctionService.GetAllAsync(ct);
        List<SignalProcessorOperationType> customFunctionOperations = customFunctions.Select(cf => new SignalProcessorOperationType
        {
            Id = cf.Id,
            Name = cf.Name,
            Type = OperationType.CustomFunction,
            InputParameters = cf.InputParameters?.Select(p => new Parameter
            {
                Name = p.Name,
                DataType = p.DataType
            }).ToList() ?? new List<Parameter>(),
            OutputParameters = cf.OutputParameters?.Select(p => new Parameter
            {
                Name = p.Name,
                DataType = p.DataType
            }).ToList() ?? new List<Parameter>()
        }).ToList();

        result.AddRange(customFunctionOperations);

        return result;
    }
}
