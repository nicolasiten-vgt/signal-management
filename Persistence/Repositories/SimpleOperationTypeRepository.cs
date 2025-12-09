using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

public class SimpleOperationTypeRepository : ISimpleOperationTypeRepository
{
    private static readonly List<SignalProcessorOperationType> Operations = new()
    {
        new SignalProcessorOperationType
        {
            Id = Guid.Parse("587159ea-13c2-45ff-a944-ffb30c2f3e88"),
            Name = "+",
            Type = OperationType.Simple,
            InputParameters = new List<Parameter>
            {
                new Parameter { Name = "a", DataType = "numeric" },
                new Parameter { Name = "b", DataType = "numeric" }
            },
            OutputParameters = new List<Parameter>
            {
                new Parameter { Name = "result", DataType = "numeric" }
            }
        },
        new SignalProcessorOperationType
        {
            Id = Guid.Parse("c503750d-bdc2-464f-94b0-a7d1c36514e0"),
            Name = "*",
            Type = OperationType.Simple,
            InputParameters = new List<Parameter>
            {
                new Parameter { Name = "a", DataType = "numeric" },
                new Parameter { Name = "b", DataType = "numeric" }
            },
            OutputParameters = new List<Parameter>
            {
                new Parameter { Name = "result", DataType = "numeric" }
            }
        },
        new SignalProcessorOperationType
        {
            Id = Guid.Parse("80c1f252-39f1-4700-9671-2eb229b706b9"),
            Name = ">",
            Type = OperationType.Simple,
            InputParameters = new List<Parameter>
            {
                new Parameter { Name = "a", DataType = "numeric" },
                new Parameter { Name = "b", DataType = "numeric" }
            },
            OutputParameters = new List<Parameter>
            {
                new Parameter { Name = "result", DataType = "string" }
            }
        },
        new SignalProcessorOperationType
        {
            Id = Guid.Parse("4e93560e-0185-4b66-a1a4-86a8634a49ea"),
            Name = "<",
            Type = OperationType.Simple,
            InputParameters = new List<Parameter>
            {
                new Parameter { Name = "a", DataType = "numeric" },
                new Parameter { Name = "b", DataType = "numeric" }
            },
            OutputParameters = new List<Parameter>
            {
                new Parameter { Name = "result", DataType = "string" }
            }
        }
    };

    public Task<IReadOnlyCollection<SignalProcessorOperationType>> GetAllAsync()
    {
        return Task.FromResult((IReadOnlyCollection<SignalProcessorOperationType>)Operations);
    }
}
