using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

public class CustomFunctionEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required ProgrammingLanguage Language { get; set; }
    public List<ParameterDefinition>? InputParameters { get; set; }
    public List<ParameterDefinition>? OutputParameters { get; set; }
    public required string SourceCode { get; set; }
    public string? Dependencies { get; set; }
}
