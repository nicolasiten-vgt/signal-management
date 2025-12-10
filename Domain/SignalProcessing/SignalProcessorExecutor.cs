using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public class SignalProcessorExecutor
{
    private readonly ISignalProcessorOperationRegistry _registry;

    public SignalProcessorExecutor(ISignalProcessorOperationRegistry registry)
    {
        _registry = registry;
    }

    public async Task<SignalProcessorExecutionResult> ExecuteAsync(
        SignalProcessor signalProcessor,
        IDictionary<string, string> inputSignalValues,
        CancellationToken ct)
    {
        var stepResults = new Dictionary<string, StepExecutionResult>();
        var stepOutputs = new Dictionary<string, IDictionary<string, string>>();
        var signalOutputs = new Dictionary<string, string>();

        var executionOrder = TopologicalSort(signalProcessor.ComputeGraph);
        foreach (var step in executionOrder)
        {
            try
            {
                if (!CanStepRun(step, stepResults))
                {
                    stepResults[step.Id] = new StepExecutionNotRun
                    {
                        Reason = "failureInDependentStep"
                    };
                    continue;
                }

                var inputs = GatherStepInputs(step, inputSignalValues, stepOutputs);
                var operation = await ResolveOperation(step.Operation, ct);
                var operationResult = operation.Execute(inputs);
                stepOutputs[step.Id] = operationResult.Outputs;
                stepResults[step.Id] = new StepExecutionSuccess
                {
                    OutputValues = stepOutputs[step.Id],
                    Logs = operationResult.Logs
                };
                WriteOutputsToSignals(step, stepOutputs[step.Id], signalOutputs);
            }
            catch (Exception ex)
            {
                // Step failed
                stepResults[step.Id] = new StepExecutionFailure
                {
                    ErrorMessage = ex.Message
                };
            }
        }

        return new SignalProcessorExecutionResult
        {
            SignalOutputs = signalOutputs,
            StepResults = stepResults
        };
    }

    private bool CanStepRun(
        ComputeStep step,
        Dictionary<string, StepExecutionResult> stepResults)
    {
        // Check all input sources of type StepOutput
        foreach (var input in step.Inputs)
        {
            if (input.Source is StepOutputInputSource stepOutputSource)
            {
                // If the dependency step has not run or failed, this step cannot run
                if (!stepResults.TryGetValue(stepOutputSource.StepId, out var depResult))
                {
                    return false;
                }

                if (depResult is not StepExecutionSuccess)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Dictionary<string, string> GatherStepInputs(
        ComputeStep step,
        IDictionary<string, string> inputSignalValues,
        Dictionary<string, IDictionary<string, string>> stepOutputs)
    {
        var inputs = new Dictionary<string, string>();

        foreach (var input in step.Inputs)
        {
            string value = input.Source switch
            {
                SignalInputSource signalSource => GetSignalValue(signalSource.SignalId, inputSignalValues),
                ConstantInputSource constantSource => constantSource.Value,
                StepOutputInputSource stepOutputSource => GetStepOutputValue(stepOutputSource, stepOutputs),
                _ => throw new InvalidOperationException($"Unknown input source type: {input.Source.GetType().Name}")
            };

            inputs[input.Name] = value;
        }

        return inputs;
    }

    private string GetSignalValue(string signalId, IDictionary<string, string> inputSignalValues)
    {
        if (!inputSignalValues.TryGetValue(signalId, out string? value))
        {
            throw new ArgumentException($"Missing required input signal: {signalId}");
        }

        return value;
    }

    private string GetStepOutputValue(
        StepOutputInputSource stepOutputSource,
        Dictionary<string, IDictionary<string, string>> stepOutputs)
    {
        if (!stepOutputs.TryGetValue(stepOutputSource.StepId, out var outputs))
        {
            throw new InvalidOperationException($"Step {stepOutputSource.StepId} has not been executed yet");
        }

        if (!outputs.TryGetValue(stepOutputSource.StepOutputId, out string? value))
        {
            throw new InvalidOperationException(
                $"Step {stepOutputSource.StepId} does not have output named {stepOutputSource.StepOutputId}");
        }

        return value;
    }

    private async Task<ISignalProcessorOperation> ResolveOperation(Operation operation, CancellationToken ct)
    {
        return operation switch
        {
            SimpleOperation simple => _registry.GetSimpleOperation(simple.Action),
            CustomFunctionOperation customFunc => await _registry.GetCustomFunctionOperationAsync(customFunc.CustomFunctionId, ct),
            _ => throw new InvalidOperationException($"Unknown operation type: {operation.GetType().Name}")
        };
    }

    private void WriteOutputsToSignals(
        ComputeStep step,
        IDictionary<string, string> outputs,
        IDictionary<string, string> signalOutputs)
    {
        foreach (var output in step.Outputs)
        {
            if (output.Targets == null) continue;

            foreach (var target in output.Targets)
            {
                if (target is SignalOutputTarget signalTarget)
                {
                    if (outputs.TryGetValue(output.Name, out string? value))
                    {
                        signalOutputs[signalTarget.SignalId] = value;
                    }
                }
            }
        }
    }

    private List<ComputeStep> TopologicalSort(List<ComputeStep> computeGraph)
    {
        var sorted = new List<ComputeStep>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        void Visit(ComputeStep step)
        {
            if (visited.Contains(step.Id)) return;
            if (visiting.Contains(step.Id))
            {
                throw new InvalidOperationException($"Cycle detected in compute graph at step {step.Id}");
            }

            visiting.Add(step.Id);

            // Visit all dependencies first
            foreach (var input in step.Inputs)
            {
                if (input.Source is StepOutputInputSource stepOutputSource)
                {
                    var depStep = computeGraph.FirstOrDefault(s => s.Id == stepOutputSource.StepId);
                    if (depStep != null)
                    {
                        Visit(depStep);
                    }
                }
            }

            visiting.Remove(step.Id);
            visited.Add(step.Id);
            sorted.Add(step);
        }

        foreach (var step in computeGraph)
        {
            Visit(step);
        }

        return sorted;
    }
}