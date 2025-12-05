using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Constants;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;

public record SignalProcessorCreateRequest(
    [Required] string Name,
    [Required] RecomputeTrigger? RecomputeTrigger,
    int? RecomputeIntervalSec,
    [Required] List<ComputeStepRequest> ComputeGraph
);

public class ComputeStepRequest
{
    [Required] public required string Id { get; set; }
    [Required] public required OperationRequest Operation { get; set; }
    [Required] public required List<InputDefinitionRequest> Inputs { get; set; }
    [Required] public required List<OutputDefinitionRequest> Outputs { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SimpleOperationRequest), typeDiscriminator: OperationTypes.Simple)]
[JsonDerivedType(typeof(CustomFunctionOperationRequest), typeDiscriminator: OperationTypes.CustomFunction)]
public abstract class OperationRequest;

public class SimpleOperationRequest : OperationRequest
{
    [Required] public required string Action { get; set; }
}

public class CustomFunctionOperationRequest : OperationRequest
{
    [Required] public required Guid CustomFunctionId { get; set; }
}

public class InputDefinitionRequest
{
    [Required] public required string Name { get; set; }
    [Required] public required string DataType { get; set; }
    [Required] public required InputSourceRequest Source { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignalInputSourceRequest), typeDiscriminator: InputSourceTypes.Signal)]
[JsonDerivedType(typeof(ConstantInputSourceRequest), typeDiscriminator: InputSourceTypes.Constant)]
[JsonDerivedType(typeof(StepOutputInputSourceRequest), typeDiscriminator: InputSourceTypes.StepOutput)]
public abstract class InputSourceRequest;

public class SignalInputSourceRequest : InputSourceRequest
{
    [Required] public required string SignalId { get; set; }
}

public class ConstantInputSourceRequest : InputSourceRequest
{
    [Required] public required string Value { get; set; }
}

public class StepOutputInputSourceRequest : InputSourceRequest
{
    [Required] public required string StepId { get; set; }
    [Required] public required string StepOutputId { get; set; }
}

public class OutputDefinitionRequest
{
    [Required] public required string Name { get; set; }
    [Required] public required string DataType { get; set; }
    public List<OutputTargetRequest>? Targets { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignalOutputTargetRequest), typeDiscriminator: OutputTargetTypes.Signal)]
public abstract class OutputTargetRequest;


public class SignalOutputTargetRequest : OutputTargetRequest
{
    [Required] public required string SignalId { get; set; }
}

