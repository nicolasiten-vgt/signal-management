using VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Mappings;

public static class DtoMappings
{
    // Signal mappings
    public static SignalDto ToDto(this Signal signal)
    {
        return new SignalDto
        {
            Id = signal.Id,
            Name = signal.Name,
            Input = signal.Input,
            Output = signal.Output,
            Unit = signal.Unit,
            DataType = signal.DataType,
            Scope = signal.Scope,
            EdgeInstance = signal.EdgeInstance,
            CreatedAt = signal.CreatedAt,
            CreatedBy = signal.CreatedBy
        };
    }

    public static IReadOnlyCollection<SignalDto> ToDto(this IEnumerable<Signal> signals)
    {
        return signals.Select(ToDto).ToList();
    }

    // CustomFunction mappings
    public static CustomFunctionDto ToDto(this CustomFunction customFunction)
    {
        return new CustomFunctionDto
        {
            Id = customFunction.Id,
            Name = customFunction.Name,
            Language = customFunction.Language,
            InputParameters = customFunction.InputParameters?.Select(ToParameterDefinitionDto).ToList(),
            OutputParameters = customFunction.OutputParameters?.Select(ToParameterDefinitionDto).ToList(),
            SourceCode = customFunction.SourceCode,
            Dependencies = customFunction.Dependencies
        };
    }

    public static IReadOnlyCollection<CustomFunctionDto> ToDto(this IEnumerable<CustomFunction> customFunctions)
    {
        return customFunctions.Select(ToDto).ToList();
    }

    private static ParameterDefinitionDto ToParameterDefinitionDto(ParameterDefinition parameter)
    {
        return new ParameterDefinitionDto
        {
            Name = parameter.Name,
            DataType = parameter.DataType
        };
    }

    // SignalProcessor mappings
    public static SignalProcessorDto ToDto(this SignalProcessor processor)
    {
        return new SignalProcessorDto
        {
            Id = processor.Id,
            Name = processor.Name,
            RecomputeTrigger = processor.RecomputeTrigger,
            RecomputeIntervalSec = processor.RecomputeIntervalSec,
            ComputeGraph = processor.ComputeGraph.Select(ToDto).ToList()
        };
    }

    public static IReadOnlyCollection<SignalProcessorDto> ToDto(this IEnumerable<SignalProcessor> processors)
    {
        return processors.Select(ToDto).ToList();
    }

    private static ComputeStepDto ToDto(this ComputeStep step)
    {
        return new ComputeStepDto
        {
            Id = step.Id,
            Operation = step.Operation.ToDto(),
            Inputs = step.Inputs.Select(ToDto).ToList(),
            Outputs = step.Outputs.Select(ToDto).ToList()
        };
    }

    private static OperationDto ToDto(this Operation operation)
    {
        return operation switch
        {
            SimpleOperation simple => new SimpleOperationDto
            {
                Action = simple.Action
            },
            CustomFunctionOperation customFunction => new CustomFunctionOperationDto
            {
                CustomFunctionId = customFunction.CustomFunctionId
            },
            _ => throw new InvalidOperationException($"Unknown operation type: {operation.GetType().Name}")
        };
    }

    private static InputDefinitionDto ToDto(this InputDefinition input)
    {
        return new InputDefinitionDto
        {
            Name = input.Name,
            DataType = input.DataType,
            Source = input.Source.ToDto()
        };
    }

    private static InputSourceDto ToDto(this InputSource source)
    {
        return source switch
        {
            SignalInputSource signal => new SignalInputSourceDto
            {
                SignalId = signal.SignalId
            },
            ConstantInputSource constant => new ConstantInputSourceDto
            {
                Value = constant.Value
            },
            StepOutputInputSource stepOutput => new StepOutputInputSourceDto
            {
                StepId = stepOutput.StepId,
                StepOutputId = stepOutput.StepOutputId
            },
            _ => throw new InvalidOperationException($"Unknown input source type: {source.GetType().Name}")
        };
    }

    private static OutputDefinitionDto ToDto(this OutputDefinition output)
    {
        return new OutputDefinitionDto
        {
            Name = output.Name,
            DataType = output.DataType,
            Targets = output.Targets?.Select(ToDto).ToList()
        };
    }

    private static OutputTargetDto ToDto(this OutputTarget target)
    {
        return target switch
        {
            SignalOutputTarget signal => new SignalOutputTargetDto
            {
                SignalId = signal.SignalId
            },
            _ => throw new InvalidOperationException($"Unknown output target type: {target.GetType().Name}")
        };
    }

    // SignalProcessorOperationType mappings
    public static SignalProcessorOperationTypeDto ToDto(this SignalProcessorOperationType operationType)
    {
        return new SignalProcessorOperationTypeDto
        {
            Id = operationType.Id,
            Name = operationType.Name,
            Type = operationType.Type,
            InputParameters = operationType.InputParameters.Select(ToParameterDto).ToList(),
            OutputParameters = operationType.OutputParameters.Select(ToParameterDto).ToList()
        };
    }

    public static IReadOnlyCollection<SignalProcessorOperationTypeDto> ToDto(this IEnumerable<SignalProcessorOperationType> operationTypes)
    {
        return operationTypes.Select(ToDto).ToList();
    }

    private static ParameterDto ToParameterDto(Parameter parameter)
    {
        return new ParameterDto
        {
            Name = parameter.Name,
            DataType = parameter.DataType
        };
    }
}
