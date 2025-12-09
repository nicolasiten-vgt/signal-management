using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test.Helpers;

public static class SignalProcessorComputeStepFactory
{
    public static ComputeStepRequest CreateSimpleStep(
        string stepId,
        string action,
        List<InputDefinitionRequest> inputs,
        List<OutputDefinitionRequest> outputs)
    {
        return new ComputeStepRequest
        {
            Id = stepId,
            Inputs = inputs,
            Operation = new SimpleOperationRequest { Action = action },
            Outputs = outputs,
        };
    }

    public static ComputeStepRequest CreateCustomFunctionStep(
        string stepId,
        Guid functionId,
        List<InputDefinitionRequest> inputs,
        List<OutputDefinitionRequest> outputs)
    { 
        return new ComputeStepRequest
        {
            Id = stepId,
            Inputs = inputs,
            Operation = new CustomFunctionOperationRequest { CustomFunctionId = functionId },
            Outputs = outputs,
        };
    }
}