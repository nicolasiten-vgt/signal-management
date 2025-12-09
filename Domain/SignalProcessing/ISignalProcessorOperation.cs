namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public interface ISignalProcessorOperation
{
    IDictionary<string, string> Execute(IDictionary<string, string> inputs);
}