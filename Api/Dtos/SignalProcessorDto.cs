using System.Text.Json.Serialization;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Constants;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;

public class SignalProcessorDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required RecomputeTrigger RecomputeTrigger { get; init; }
    public int? RecomputeIntervalSec { get; init; }
    public required List<ComputeStepDto> ComputeGraph { get; init; }
}

public class ComputeStepDto
{
    public required string Id { get; set; }
    public required OperationDto Operation { get; set; }
    public required List<InputDefinitionDto> Inputs { get; set; }
    public required List<OutputDefinitionDto> Outputs { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SimpleOperationDto), typeDiscriminator: OperationTypes.Simple)]
[JsonDerivedType(typeof(CustomFunctionOperationDto), typeDiscriminator: OperationTypes.CustomFunction)]
public abstract class OperationDto;

public class SimpleOperationDto : OperationDto
{
    public required string Action { get; set; }
}

public class CustomFunctionOperationDto : OperationDto
{
    public required Guid CustomFunctionId { get; set; }
}

public class InputDefinitionDto
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
    public required InputSourceDto Source { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignalInputSourceDto), typeDiscriminator: InputSourceTypes.Signal)]
[JsonDerivedType(typeof(ConstantInputSourceDto), typeDiscriminator: InputSourceTypes.Constant)]
[JsonDerivedType(typeof(StepOutputInputSourceDto), typeDiscriminator: InputSourceTypes.StepOutput)]
public abstract class InputSourceDto;

public class SignalInputSourceDto : InputSourceDto
{
    public required string SignalId { get; set; }
}

public class ConstantInputSourceDto : InputSourceDto
{
    public required string Value { get; set; }
}

public class StepOutputInputSourceDto : InputSourceDto
{
    public required string StepId { get; set; }
    public required string StepOutputId { get; set; }
}

public class OutputDefinitionDto
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
    public List<OutputTargetDto>? Targets { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignalOutputTargetDto), typeDiscriminator: OutputTargetTypes.Signal)]
public abstract class OutputTargetDto;

public class SignalOutputTargetDto : OutputTargetDto
{
    public required string SignalId { get; set; }
}
