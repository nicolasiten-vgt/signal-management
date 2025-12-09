using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test.Helpers;

public static class SignalProcessorOutputDefinitionFactory
{
    public static OutputDefinitionRequest CreateOutput(string name, string dataType, string? outputSignalId = null)
    {
        List<OutputTargetRequest>? targets = !string.IsNullOrEmpty(outputSignalId)
            ? [new SignalOutputTargetRequest { SignalId = outputSignalId }]
            : null;

        return new OutputDefinitionRequest
        {
            Name = name,
            DataType = dataType,
            Targets = targets
        };
    }
}