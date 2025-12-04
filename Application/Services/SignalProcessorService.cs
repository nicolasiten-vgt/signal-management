using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class SignalProcessorService : ISignalProcessorService
{
    private readonly ISignalProcessorRepository _repository;
    private readonly ISignalRepository _signalRepository;

    public SignalProcessorService(ISignalProcessorRepository repository, ISignalRepository signalRepository)
    {
        _repository = repository;
        _signalRepository = signalRepository;
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
        var signalProcessor = new SignalProcessor(
            request.Name,
            request.RecomputeTrigger!.Value,
            request.RecomputeIntervalSec,
            computeGraph
        );

        // Validate all signal inputs reference existing signals
        var signalInputs = signalProcessor.ComputeGraph
            .SelectMany(step => step.Inputs)
            .Where(input => input.Source is SignalInputSource)
            .Select(input => ((SignalInputSource)input.Source).SignalId)
            .Distinct()
            .ToList();

        foreach (var signalId in signalInputs)
        {
            var signal = await _signalRepository.GetByIdAsync(signalId, ct);
            if (signal == null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["ComputeGraph"] = new[] { $"Signal input references non-existent signal ID: {signalId}" }
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
                Type = simple.Type,
                Action = simple.Action
            },
            CustomFunctionOperationRequest customFunction => new CustomFunctionOperation
            {
                Type = customFunction.Type,
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
                Type = signal.Type,
                SignalId = signal.SignalId
            },
            ConstantInputSourceRequest constant => new ConstantInputSource
            {
                Type = constant.Type,
                Value = constant.Value
            },
            StepOutputInputSourceRequest stepOutput => new StepOutputInputSource
            {
                Type = stepOutput.Type,
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
                Type = signal.Type,
                SignalId = signal.SignalId
            },
            _ => throw new InvalidOperationException($"Unknown output target type: {targetRequest.GetType().Name}")
        };
    }
}
