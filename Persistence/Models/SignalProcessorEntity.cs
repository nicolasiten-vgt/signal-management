using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

public class SignalProcessorEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required RecomputeTrigger RecomputeTrigger { get; set; }
    public int? RecomputeIntervalSec { get; set; }
    public required List<ComputeStep> ComputeGraph { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
}
