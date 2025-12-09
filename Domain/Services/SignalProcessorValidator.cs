using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Services;

public static class SignalProcessorValidator
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

    public static void ValidateStepIdsUnique(List<ComputeStep> computeGraph)
    {
        if (computeGraph.Count == 0)
        {
            return;
        }

        var duplicateIds = computeGraph
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ComputeGraph"] = new[]
                {
                    $"Compute step IDs must be unique. Duplicates: {string.Join(", ", duplicateIds)}"
                }
            });
        }
    }

    public static void ValidateGraphConnected(List<ComputeStep> computeGraph)
    {
        if (computeGraph.Count == 0)
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
        if (computeGraph.Count == 0)
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
        if (computeGraph.Count == 0)
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
        if (computeGraph.Count == 0)
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

    public static void ValidateStepOperationDefinitions(
        List<ComputeStep> computeGraph,
        IReadOnlyCollection<SignalProcessorOperationType> operationTypes)
    {
        if (computeGraph.Count == 0)
        {
            return;
        }

        foreach (ComputeStep step in computeGraph)
        {
            // 1. Resolve operation type
            SignalProcessorOperationType? operationType = step.Operation switch
            {
                SimpleOperation simple => operationTypes.FirstOrDefault(t =>
                    t.Type == OperationType.Simple &&
                    string.Equals(t.Name, simple.Action, StringComparison.OrdinalIgnoreCase)),
                CustomFunctionOperation custom => operationTypes.FirstOrDefault(t =>
                    t.Type == OperationType.CustomFunction &&
                    t.Id == custom.CustomFunctionId),
                _ => null
            };

            if (operationType == null)
            {
                string operationDescription = step.Operation switch
                {
                    SimpleOperation simple => $"simple operation '{simple.Action}'",
                    CustomFunctionOperation custom => $"custom function with ID '{custom.CustomFunctionId}'",
                    _ => "unknown operation"
                };
                
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["ComputeGraph"] = new[]
                    {
                        $"Step '{step.Id}': Operation definition not found for {operationDescription}."
                    }
                });
            }

            // 2. Validate inputs
            var parameterInputs = operationType.InputParameters
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (InputDefinition input in step.Inputs)
            {
                // Check if input is defined by the operation
                if (!parameterInputs.TryGetValue(input.Name, out Parameter? param))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["ComputeGraph"] = new[]
                        {
                            $"Step '{step.Id}': Input '{input.Name}' is not defined by the operation '{operationType.Name}'."
                        }
                    });
                }

                // Check data type match (case-insensitive)
                if (!string.Equals(input.DataType, param.DataType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["ComputeGraph"] = new[]
                        {
                            $"Step '{step.Id}': Input '{input.Name}' has data type '{input.DataType}' " +
                            $"but operation '{operationType.Name}' expects '{param.DataType}'."
                        }
                    });
                }
            }

            // Check for missing required inputs
            var definedInputNames = new HashSet<string>(
                step.Inputs.Select(i => i.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (Parameter param in operationType.InputParameters)
            {
                if (!definedInputNames.Contains(param.Name))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["ComputeGraph"] = new[]
                        {
                            $"Step '{step.Id}': Required input '{param.Name}' is missing for operation '{operationType.Name}'."
                        }
                    });
                }
            }

            // 3. Validate outputs
            var parameterOutputs = operationType.OutputParameters
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (OutputDefinition output in step.Outputs)
            {
                // Check if output is defined by the operation
                if (!parameterOutputs.TryGetValue(output.Name, out Parameter? param))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["ComputeGraph"] = new[]
                        {
                            $"Step '{step.Id}': Output '{output.Name}' is not defined by the operation '{operationType.Name}'."
                        }
                    });
                }

                // Check data type match (case-insensitive)
                if (!string.Equals(output.DataType, param.DataType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["ComputeGraph"] = new[]
                        {
                            $"Step '{step.Id}': Output '{output.Name}' has data type '{output.DataType}' " +
                            $"but operation '{operationType.Name}' expects '{param.DataType}'."
                        }
                    });
                }
            }

            // Check for missing required outputs
            var definedOutputNames = new HashSet<string>(
                step.Outputs.Select(o => o.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (Parameter param in operationType.OutputParameters)
            {
                if (!definedOutputNames.Contains(param.Name))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["ComputeGraph"] = new[]
                        {
                            $"Step '{step.Id}': Required output '{param.Name}' is missing for operation '{operationType.Name}'."
                        }
                    });
                }
            }
        }
    }
}
