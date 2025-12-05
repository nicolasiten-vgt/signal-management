using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;

public class SignalDto
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required bool Input { get; set; }
    public required bool Output { get; set; }
    public required string Unit { get; set; }
    public required SignalDataType DataType { get; set; }
    public required SignalScope Scope { get; set; }
    public string? EdgeInstance { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}
