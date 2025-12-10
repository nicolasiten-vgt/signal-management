using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public interface ISignalProcessorSimpleOperation : ISignalProcessorOperation
{
    SignalProcessorOperationType OperationType { get; }
}
