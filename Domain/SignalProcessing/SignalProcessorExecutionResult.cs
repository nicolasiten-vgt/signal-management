namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public class SignalProcessorExecutionResult
{
    public IDictionary<string, string> SignalOutputs { get; init; } = new Dictionary<string, string>();

    public required IDictionary<string, StepExecutionResult> StepResults { get; init; }
}

public abstract class StepExecutionResult
{
}

public class StepExecutionSuccess : StepExecutionResult
{
    public required IDictionary<string, string> OutputValues { get; init; }

    public string? Logs { get; init; }
}

public class StepExecutionFailure : StepExecutionResult
{
    public required string ErrorMessage { get; init; }

    public string? Logs { get; init; }
}

public class StepExecutionNotRun : StepExecutionResult
{
    public required string Reason { get; init; }
}
