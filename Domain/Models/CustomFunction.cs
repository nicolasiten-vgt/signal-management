namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

public enum ProgrammingLanguage
{
    Csharp,
    JavaScript
}

public class ParameterDefinition
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
}

public class CustomFunction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required ProgrammingLanguage Language { get; set; }
    public List<ParameterDefinition>? InputParameters { get; set; }
    public List<ParameterDefinition>? OutputParameters { get; set; }
    public required string SourceCode { get; set; }
    public string? Dependencies { get; set; }
}
