namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

public enum OperationType
{
    Simple,
    CustomFunction
}

public class Parameter
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
}

public class SignalProcessorOperationType
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required OperationType Type { get; set; }
    public required List<Parameter> InputParameters { get; set; }
    public required List<Parameter> OutputParameters { get; set; }
}
