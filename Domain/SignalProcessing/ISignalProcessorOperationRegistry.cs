namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public interface ISignalProcessorOperationRegistry
{
    ISignalProcessorOperation GetSimpleOperation(string action);

    Task<ISignalProcessorOperation> GetCustomFunctionOperationAsync(Guid customFunctionId, CancellationToken ct);
}
