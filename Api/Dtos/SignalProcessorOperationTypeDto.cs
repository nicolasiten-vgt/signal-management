using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;

public class ParameterDto
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
}

public class SignalProcessorOperationTypeDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required OperationType Type { get; set; }
    public required List<ParameterDto> InputParameters { get; set; }
    public required List<ParameterDto> OutputParameters { get; set; }
}
