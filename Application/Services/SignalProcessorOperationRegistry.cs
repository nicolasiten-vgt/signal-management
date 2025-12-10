using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class SignalProcessorOperationRegistry : ISignalProcessorOperationRegistry
{
    private readonly ICustomFunctionRepository _customFunctionRepository;
    private readonly Dictionary<string, ISignalProcessorSimpleOperation> _simpleOperations;

    public SignalProcessorOperationRegistry(ICustomFunctionRepository customFunctionRepository)
    {
        _customFunctionRepository = customFunctionRepository;
        _simpleOperations = SignalProcessorSimpleOperations
            .All
            .ToDictionary(
                key => key.OperationType.Name, 
                element => element);
    }

    public ISignalProcessorOperation GetSimpleOperation(string action)
    {
        if (_simpleOperations.TryGetValue(action, out var operation))
        {
            return operation;
        }
        
        throw new ArgumentException($"Unknown simple operation action: {action}");
    }

    public async Task<ISignalProcessorOperation> GetCustomFunctionOperationAsync(Guid customFunctionId, CancellationToken ct)
    {
        var customFunction = await _customFunctionRepository.GetByIdAsync(customFunctionId, ct);
        
        if (customFunction == null)
        {
            throw new NotFoundException("CustomFunction", customFunctionId.ToString());
        }
        
        return customFunction.Language switch
        {
            Domain.Models.ProgrammingLanguage.JavaScript => new JavaScriptCustomFunctionOperation(customFunction
                .SourceCode),
            _ => throw new ArgumentException($"Unsupported custom function language: {customFunction.Language}.")
        };
    }
}
