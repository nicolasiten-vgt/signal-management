namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

public enum SignalDataType
{
    Numeric,
    String
}

public enum SignalScope
{
    Global,
    Edge,
    Tenant
}

public class Signal
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required bool Input { get; set; }
    public required bool Output { get; set; }
    public required string Unit { get; set; }
    public required SignalDataType DataType { get; set; }
    public required SignalScope Scope { get; set; }
    public string? EdgeInstance { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; init; } = "test@vgt.energy";
}