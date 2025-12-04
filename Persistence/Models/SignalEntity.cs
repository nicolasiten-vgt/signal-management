using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

public class SignalEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool Input { get; set; }
    public required bool Output { get; set; }
    public required string Unit { get; set; }
    public required SignalDataType DataType { get; set; }
    public required SignalScope Scope { get; set; }
    public string? EdgeInstance { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
}
