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

    public async Task<IReadOnlyCollection<SignalProcessorOperationType>> GetAllAsync(CancellationToken ct = default)
    {
        var simpleOperations = await _simpleOperationTypeRepository.GetAllAsync();
        var customFunctions = await _customFunctionService.GetAllAsync(ct);

        var signalProcessorOperationTypes = new SignalProcessorOperationTypes(
            simpleOperations.Select(so => so.OperationType).ToArray(), 
            customFunctions);
        return signalProcessorOperationTypes.All;
    }
}
