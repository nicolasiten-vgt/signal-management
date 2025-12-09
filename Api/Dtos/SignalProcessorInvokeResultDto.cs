using System.Text.Json.Serialization;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;

public class SignalProcessorInvokeResultDto
{
    public IDictionary<string, string>? SignalOutputs { get; init; }

    public required IDictionary<string, StepInvocationResultDto> StepResults { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StepInvocationSuccessResultDto), typeDiscriminator: "success")]
[JsonDerivedType(typeof(StepInvocationFailureResultDto), typeDiscriminator: "failure")]
[JsonDerivedType(typeof(StepInvocationNotRunResultDto), typeDiscriminator: "notRun")]
public abstract class StepInvocationResultDto
{
}

public class StepInvocationSuccessResultDto : StepInvocationResultDto
{
    public required IDictionary<string, string> OutputValues { get; init; }

    public string? Logs { get; init; }
}

public class StepInvocationFailureResultDto : StepInvocationResultDto
{
    public required string ErrorMessage { get; init; }

    public string? Logs { get; init; }
}

public class StepInvocationNotRunResultDto : StepInvocationResultDto
{
    public required string NotRunReason { get; init; }
}
