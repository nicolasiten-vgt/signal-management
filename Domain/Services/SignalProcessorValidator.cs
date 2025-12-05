using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Services;

public class SignalProcessorValidator
{
    public static void ValidateRecomputeIntervalRules(SignalProcessor processor)
    {
        if (processor.RecomputeTrigger == RecomputeTrigger.Interval)
        {
            if (processor.RecomputeIntervalSec == null || processor.RecomputeIntervalSec <= 0)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["RecomputeIntervalSec"] = new[] { "RecomputeIntervalSec must be set and greater than 0 when RecomputeTrigger is Interval" }
                });
            }
        }
        else
        {
            if (processor.RecomputeIntervalSec != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["RecomputeIntervalSec"] = new[] { "RecomputeIntervalSec must not be set unless RecomputeTrigger is Interval" }
                });
            }
        }
    }

    public static void ValidateGraphConnected(List<ComputeStep> computeGraph)
    {
        if (computeGraph == null || computeGraph.Count == 0)
        {
            return;
        }

        var stepIds = computeGraph.Select(s => s.Id).ToHashSet();
        var reachable = new HashSet<string>();

        // Start DFS from the first step (arbitrary choice)
        // If the graph is connected, we should reach all nodes from any starting point
        DfsVisitBidirectional(computeGraph[0], computeGraph, reachable);

        if (reachable.Count != stepIds.Count)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ComputeGraph"] = new[] { "ComputeGraph must be one connected graph" }
            });
        }
    }

    private static void DfsVisitBidirectional(ComputeStep step, List<ComputeStep> allSteps, HashSet<string> reachable)
    {
        if (reachable.Contains(step.Id))
        {
            return;
        }

        reachable.Add(step.Id);

        // Traverse forward: find all steps that depend on this step's outputs
        foreach (var otherStep in allSteps)
        {
            foreach (var input in otherStep.Inputs)
            {
                if (input.Source is StepOutputInputSource sos && sos.StepId == step.Id)
                {
                    DfsVisitBidirectional(otherStep, allSteps, reachable);
                }
            }
        }

        // Traverse backward: find all steps that this step depends on
        foreach (var input in step.Inputs)
        {
            if (input.Source is StepOutputInputSource sos)
            {
                var dependsOnStep = allSteps.FirstOrDefault(s => s.Id == sos.StepId);
                if (dependsOnStep != null)
                {
                    DfsVisitBidirectional(dependsOnStep, allSteps, reachable);
                }
            }
        }
    }

    public static void ValidateGraphAcyclic(List<ComputeStep> computeGraph)
    {
        if (computeGraph == null || computeGraph.Count == 0)
        {
            return;
        }

        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var step in computeGraph)
        {
            if (HasCycle(step, computeGraph, visited, recursionStack))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["ComputeGraph"] = new[] { "ComputeGraph must be acyclic (no cycles allowed)" }
                });
            }
        }
    }

    private static bool HasCycle(ComputeStep step, List<ComputeStep> allSteps, HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(step.Id))
        {
            return true;
        }

        if (visited.Contains(step.Id))
        {
            return false;
        }

        visited.Add(step.Id);
        recursionStack.Add(step.Id);

        // Check all steps this step depends on
        foreach (var input in step.Inputs)
        {
            if (input.Source is StepOutputInputSource sos)
            {
                var dependsOnStep = allSteps.FirstOrDefault(s => s.Id == sos.StepId);
                if (dependsOnStep != null && HasCycle(dependsOnStep, allSteps, visited, recursionStack))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(step.Id);
        return false;
    }

    public static void ValidateStepOutputReferences(List<ComputeStep> computeGraph)
    {
        if (computeGraph == null || computeGraph.Count == 0)
        {
            return;
        }

        var stepOutputMap = new Dictionary<string, HashSet<string>>();
        foreach (var step in computeGraph)
        {
            stepOutputMap[step.Id] = step.Outputs.Select(o => o.Name).ToHashSet();
        }

        foreach (var step in computeGraph)
        {
            foreach (var input in step.Inputs)
            {
                if (input.Source is StepOutputInputSource sos)
                {
                    if (!stepOutputMap.ContainsKey(sos.StepId))
                    {
                        throw new ValidationException(new Dictionary<string, string[]>
                        {
                            ["ComputeGraph"] = new[] { $"Step output reference not found: step '{sos.StepId}' does not exist" }
                        });
                    }
                    if (!stepOutputMap[sos.StepId].Contains(sos.StepOutputId))
                    {
                        throw new ValidationException(new Dictionary<string, string[]>
                        {
                            ["ComputeGraph"] = new[] { $"Step output reference not found: output '{sos.StepOutputId}' does not exist in step '{sos.StepId}'" }
                        });
                    }
                }
            }
        }
    }

    public static void ValidateDataTypeMatches(List<ComputeStep> computeGraph)
    {
        if (computeGraph == null || computeGraph.Count == 0)
        {
            return;
        }

        var stepOutputTypes = new Dictionary<(string stepId, string outputName), string>();
        foreach (var step in computeGraph)
        {
            foreach (var output in step.Outputs)
            {
                stepOutputTypes[(step.Id, output.Name)] = output.DataType;
            }
        }

        foreach (var step in computeGraph)
        {
            foreach (var input in step.Inputs)
            {
                if (input.Source is StepOutputInputSource sos)
                {
                    var key = (sos.StepId, sos.StepOutputId);
                    if (stepOutputTypes.TryGetValue(key, out var outputDataType))
                    {
                        if (input.DataType != outputDataType)
                        {
                            throw new ValidationException(new Dictionary<string, string[]>
                            {
                                ["ComputeGraph"] = new[] { $"Data type mismatch: input '{input.Name}' expects '{input.DataType}' but step output '{sos.StepOutputId}' from step '{sos.StepId}' is '{outputDataType}'" }
                            });
                        }
                    }
                }
            }
        }
    }
}
