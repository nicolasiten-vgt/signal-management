using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class SignalProcessorOperationRegistry : ISignalProcessorOperationRegistry
{
    private readonly ICustomFunctionRepository _customFunctionRepository;
    private readonly Dictionary<string, ISignalProcessorOperation> _simpleOperations;

    public SignalProcessorOperationRegistry(ICustomFunctionRepository customFunctionRepository)
    {
        _customFunctionRepository = customFunctionRepository;
        
        // Register all simple operations
        _simpleOperations = new Dictionary<string, ISignalProcessorOperation>(StringComparer.OrdinalIgnoreCase)
        {
            ["+"] = new AddOperation(),
            ["*"] = new MultiplyOperation(),
            [">"] = new BiggerThanOperation(),
            ["<"] = new LessThanOperation()
        };
    }

    public ISignalProcessorOperation GetSimpleOperation(string action)
    {
        if (_simpleOperations.TryGetValue(action, out ISignalProcessorOperation? operation))
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

        // For now, only JavaScript is supported
        if (customFunction.Language != Domain.Models.ProgrammingLanguage.JavaScript)
        {
            throw new ArgumentException($"Unsupported custom function language: {customFunction.Language}. Only JavaScript is supported.");
        }

        return new JavaScriptCustomFunctionOperation(customFunction.SourceCode);
    }
}
