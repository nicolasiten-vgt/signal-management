using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;

public class ParameterDefinitionDto
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
}

public class CustomFunctionDto
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required ProgrammingLanguage Language { get; set; }
    public List<ParameterDefinitionDto>? InputParameters { get; set; }
    public List<ParameterDefinitionDto>? OutputParameters { get; set; }
    public required string SourceCode { get; set; }
    public string? Dependencies { get; set; }
}
