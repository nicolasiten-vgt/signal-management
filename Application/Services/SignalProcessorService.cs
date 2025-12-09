using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class SignalProcessorService : ISignalProcessorService
{
    private readonly ISignalProcessorRepository _repository;
    private readonly ISignalRepository _signalRepository;
    private readonly ISignalProcessorOperationRegistry _operationRegistry;
    private readonly ISignalProcessorOperationTypeService _operationTypeService;

    public SignalProcessorService(
        ISignalProcessorRepository repository,
        ISignalRepository signalRepository,
        ISignalProcessorOperationRegistry operationRegistry,
        ISignalProcessorOperationTypeService operationTypeService)
    {
        _repository = repository;
        _signalRepository = signalRepository;
        _operationRegistry = operationRegistry;
        _operationTypeService = operationTypeService;
    }

    public async Task<SignalProcessor> CreateAsync(SignalProcessorCreateRequest request, CancellationToken ct)
    {
        // Check name uniqueness
        var existing = await _repository.GetByNameAsync(request.Name, ct);
        if (existing != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Name"] = new[] { $"A SignalProcessor with name '{request.Name}' already exists" }
            });
        }

        var computeGraph = MapComputeGraph(request.ComputeGraph);
        
        var operationTypes = await _operationTypeService.GetAllAsync(ct);
        var signalProcessor = new SignalProcessor(
            request.Name,
            request.RecomputeTrigger!.Value,
            request.RecomputeIntervalSec,
            computeGraph,
            operationTypes);

        // Validate all signal inputs reference existing signals and have matching data types
        var signalInputs = signalProcessor.ComputeGraph
            .SelectMany(step => step.Inputs)
            .Where(input => input.Source is SignalInputSource)
            .Select(input => new { Input = input, SignalId = ((SignalInputSource)input.Source).SignalId })
            .ToList();

        foreach (var signalInput in signalInputs)
        {
            var signal = await _signalRepository.GetByIdAsync(signalInput.SignalId, ct);
            if (signal == null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["ComputeGraph"] = new[] { $"Signal input references non-existent signal ID: {signalInput.SignalId}" }
                });
            }

            // Validate data type matches
            var expectedDataType = signal.DataType.ToString();
            if (!signalInput.Input.DataType.Equals(expectedDataType, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["ComputeGraph"] = new[] { $"Signal input '{signalInput.Input.Name}' has data type '{signalInput.Input.DataType}' but signal '{signal.Name}' (ID: {signalInput.SignalId}) has data type '{expectedDataType}'" }
                });
            }
        }

        return await _repository.CreateAsync(signalProcessor, ct);
    }

    public async Task<IReadOnlyCollection<SignalProcessor>> GetAllAsync(CancellationToken ct)
    {
        return await _repository.GetAllAsync(ct);
    }

    public async Task<SignalProcessor> GetByIdAsync(string id, CancellationToken ct)
    {
        var signalProcessor = await _repository.GetByIdAsync(id, ct);
        if (signalProcessor == null)
        {
            throw new NotFoundException(nameof(SignalProcessor), id);
        }

        return signalProcessor;
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        if (!deleted)
        {
            throw new NotFoundException(nameof(SignalProcessor), id);
        }
    }

    public async Task<SignalProcessorExecutionResult> InvokeAsync(string id, IDictionary<string, string> signalInputs, CancellationToken ct)
    {
        var signalProcessor = await GetByIdAsync(id, ct);
        await ValidateInputSignals(signalProcessor, signalInputs, ct);

        var executor = new SignalProcessorExecutor(_operationRegistry);
        var result = await executor.ExecuteAsync(signalProcessor, signalInputs, ct);

        return result;
    }

    private async Task ValidateInputSignals(
        SignalProcessor signalProcessor,
        IDictionary<string, string> providedInputs,
        CancellationToken ct)
    {
        // Find all signal inputs referenced in the compute graph
        var signalInputs = signalProcessor.ComputeGraph
            .SelectMany(step => step.Inputs)
            .Where(input => input.Source is SignalInputSource)
            .Select(input => new
            {
                Input = input,
                SignalId = ((SignalInputSource)input.Source).SignalId
            })
            .ToList();

        var errors = new Dictionary<string, List<string>>();

        foreach (var signalInput in signalInputs)
        {
            // Check if the signal value is provided
            if (!providedInputs.ContainsKey(signalInput.SignalId))
            {
                if (!errors.ContainsKey("InputSignals"))
                {
                    errors["InputSignals"] = new List<string>();
                }
                errors["InputSignals"].Add($"Missing required input signal: {signalInput.SignalId}");
                continue;
            }

            // Get the signal definition to validate the data type
            var signal = await _signalRepository.GetByIdAsync(signalInput.SignalId, ct);
            if (signal == null)
            {
                if (!errors.ContainsKey("InputSignals"))
                {
                    errors["InputSignals"] = new List<string>();
                }
                errors["InputSignals"].Add($"Signal {signalInput.SignalId} not found");
                continue;
            }

            // Validate the data type
            string providedValue = providedInputs[signalInput.SignalId];
            if (!ValidateDataType(signal.DataType.ToString(), providedValue))
            {
                if (!errors.ContainsKey("InputSignals"))
                {
                    errors["InputSignals"] = new List<string>();
                }
                errors["InputSignals"].Add(
                    $"Invalid value '{providedValue}' for signal {signalInput.SignalId} (expected {signal.DataType})");
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
        }
    }

    private bool ValidateDataType(string dataType, string value)
    {
        return dataType.ToLower() switch
        {
            "numeric" => decimal.TryParse(value, out _),
            "string" => true, 
            _ => true 
        };
    }

    private static List<ComputeStep> MapComputeGraph(List<ComputeStepRequest> computeGraphRequest)
    {
        return computeGraphRequest.Select(MapComputeStep).ToList();
    }

    private static ComputeStep MapComputeStep(ComputeStepRequest stepRequest)
    {
        return new ComputeStep
        {
            Id = stepRequest.Id,
            Operation = MapOperation(stepRequest.Operation),
            Inputs = stepRequest.Inputs.Select(MapInputDefinition).ToList(),
            Outputs = stepRequest.Outputs.Select(MapOutputDefinition).ToList()
        };
    }

    private static Operation MapOperation(OperationRequest operationRequest)
    {
        return operationRequest switch
        {
            SimpleOperationRequest simple => new SimpleOperation
            {
                Action = simple.Action
            },
            CustomFunctionOperationRequest customFunction => new CustomFunctionOperation
            {
                CustomFunctionId = customFunction.CustomFunctionId
            },
            _ => throw new InvalidOperationException($"Unknown operation type: {operationRequest.GetType().Name}")
        };
    }

    private static InputDefinition MapInputDefinition(InputDefinitionRequest inputRequest)
    {
        return new InputDefinition
        {
            Name = inputRequest.Name,
            DataType = inputRequest.DataType,
            Source = MapInputSource(inputRequest.Source)
        };
    }

    private static InputSource MapInputSource(InputSourceRequest sourceRequest)
    {
        return sourceRequest switch
        {
            SignalInputSourceRequest signal => new SignalInputSource
            {
                SignalId = signal.SignalId
            },
            ConstantInputSourceRequest constant => new ConstantInputSource
            {
                Value = constant.Value
            },
            StepOutputInputSourceRequest stepOutput => new StepOutputInputSource
            {
                StepId = stepOutput.StepId,
                StepOutputId = stepOutput.StepOutputId
            },
            _ => throw new InvalidOperationException($"Unknown input source type: {sourceRequest.GetType().Name}")
        };
    }

    private static OutputDefinition MapOutputDefinition(OutputDefinitionRequest outputRequest)
    {
        return new OutputDefinition
        {
            Name = outputRequest.Name,
            DataType = outputRequest.DataType,
            Targets = outputRequest.Targets?.Select(MapOutputTarget).ToList()
        };
    }

    private static OutputTarget MapOutputTarget(OutputTargetRequest targetRequest)
    {
        return targetRequest switch
        {
            SignalOutputTargetRequest signal => new SignalOutputTarget
            {
                SignalId = signal.SignalId
            },
            _ => throw new InvalidOperationException($"Unknown output target type: {targetRequest.GetType().Name}")
        };
    }
}
