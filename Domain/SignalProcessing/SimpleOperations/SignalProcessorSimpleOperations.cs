namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

public static class SignalProcessorSimpleOperations
{
    public static IReadOnlyCollection<ISignalProcessorSimpleOperation> All { get; } = new List<ISignalProcessorSimpleOperation>
    {
        new AddOperation(),
        new MultiplyOperation(),
        new BiggerThanOperation(),
        new LessThanOperation()
    };
}