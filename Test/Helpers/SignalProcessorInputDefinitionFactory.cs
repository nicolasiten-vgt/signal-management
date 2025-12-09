using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test.Helpers;

public static class SignalProcessorInputDefinitionFactory
{
    public static InputDefinitionRequest CreateSignalInput(string name, string dataType, string signalId)
    {
        return new InputDefinitionRequest
        {
            Name = name,
            DataType = dataType,
            Source = new SignalInputSourceRequest { SignalId = signalId }
        };
    }

    public static InputDefinitionRequest CreateStepOutputInput(
        string name,
        string dataType,
        string stepId,
        string stepOutputId)
    {
        return new InputDefinitionRequest
        {
            Name = name,
            DataType = dataType,
            Source = new StepOutputInputSourceRequest
            {
                StepId = stepId,
                StepOutputId = stepOutputId
            }
        };
    }

    public static InputDefinitionRequest CreateConstantInput(string name, string dataType, string value)
    
    {
        return new InputDefinitionRequest
        {
            Name = name,
            DataType = dataType,
            Source = new ConstantInputSourceRequest { Value = value }
        };
    }
}