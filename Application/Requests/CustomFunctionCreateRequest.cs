using System.ComponentModel.DataAnnotations;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;

public record CustomFunctionCreateRequest(
    [Required] string Name,
    [Required] ProgrammingLanguage? Language,
    List<ParameterDefinition>? InputParameters,
    List<ParameterDefinition>? OutputParameters,
    [Required] string SourceCode,
    string? Dependencies
);
