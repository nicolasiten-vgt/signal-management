using System.ComponentModel.DataAnnotations;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;

public record SignalCreateRequest(
    [Required] string Id,
    [Required] string Name,
    [Required] bool? Input,
    [Required] bool? Output,
    [Required] string Unit,
    [Required] SignalDataType? DataType,
    [Required] SignalScope? Scope,
    string? EdgeInstance
);