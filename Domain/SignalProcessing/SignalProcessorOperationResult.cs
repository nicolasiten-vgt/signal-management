namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

public class SignalProcessorOperationResult
{
    public required IDictionary<string, string> Outputs { get; init; }
    
    public string? Logs { get; init; }
}