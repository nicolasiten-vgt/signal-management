using System.Text.Json.Serialization;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Constants;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Services;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

public enum RecomputeTrigger
{
    Interval,
    SignalChange
}

public class SignalProcessor
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public RecomputeTrigger RecomputeTrigger { get; private set; }
    public int? RecomputeIntervalSec { get; private set; }
    public List<ComputeStep> ComputeGraph { get; private set; }

    public SignalProcessor(string name, RecomputeTrigger recomputeTrigger, int? recomputeIntervalSec, List<ComputeStep> computeGraph)
    {
        Id = Guid.NewGuid();
        Name = name;
        RecomputeTrigger = recomputeTrigger;
        RecomputeIntervalSec = recomputeIntervalSec;
        ComputeGraph = computeGraph;

        // Run all domain validations
        SignalProcessorValidator.ValidateRecomputeIntervalRules(this);
        SignalProcessorValidator.ValidateGraphAcyclic(computeGraph);
        SignalProcessorValidator.ValidateGraphConnected(computeGraph);
        SignalProcessorValidator.ValidateStepOutputReferences(computeGraph);
        SignalProcessorValidator.ValidateDataTypeMatches(computeGraph);
    }
    
    private SignalProcessor(Guid id, string name, RecomputeTrigger recomputeTrigger, int? recomputeIntervalSec, List<ComputeStep> computeGraph)
    {
        Id = id;
        Name = name;
        RecomputeTrigger = recomputeTrigger;
        RecomputeIntervalSec = recomputeIntervalSec;
        ComputeGraph = computeGraph;
    }
    
    public static SignalProcessor Restore(Guid id, string name, RecomputeTrigger recomputeTrigger, int? recomputeIntervalSec, List<ComputeStep> computeGraph)
    {
        return new SignalProcessor(id, name, recomputeTrigger, recomputeIntervalSec, computeGraph);
    }
}

public class ComputeStep
{
    public required string Id { get; set; }
    public required Operation Operation { get; set; }
    public required List<InputDefinition> Inputs { get; set; }
    public required List<OutputDefinition> Outputs { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SimpleOperation), typeDiscriminator: OperationTypes.Simple)]
[JsonDerivedType(typeof(CustomFunctionOperation), typeDiscriminator: OperationTypes.CustomFunction)]
public abstract class Operation;

public class SimpleOperation : Operation
{
    public required string Action { get; set; }
}

public class CustomFunctionOperation : Operation
{
    public required Guid CustomFunctionId { get; set; }
}

public class InputDefinition
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
    public required InputSource Source { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignalInputSource), typeDiscriminator: InputSourceTypes.Signal)]
[JsonDerivedType(typeof(ConstantInputSource), typeDiscriminator: InputSourceTypes.Constant)]
[JsonDerivedType(typeof(StepOutputInputSource), typeDiscriminator: InputSourceTypes.StepOutput)]
public abstract class InputSource;

public class SignalInputSource : InputSource
{
    public required string SignalId { get; set; }
}

public class ConstantInputSource : InputSource
{
    public required string Value { get; set; }
}

public class StepOutputInputSource : InputSource
{
    public required string StepId { get; set; }
    public required string StepOutputId { get; set; }
}

public class OutputDefinition
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
    public List<OutputTarget>? Targets { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignalOutputTarget), typeDiscriminator: OutputTargetTypes.Signal)]
public abstract class OutputTarget;

public class SignalOutputTarget : OutputTarget
{
    public required string SignalId { get; set; }
}
