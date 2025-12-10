namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public interface ISignalProcessorOperation
{
    SignalProcessorOperationResult Execute(IDictionary<string, string> inputs);
}