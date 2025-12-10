using System.Globalization;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

public class AddOperation : ISignalProcessorSimpleOperation
{
    public SignalProcessorOperationType OperationType { get; } = new SignalProcessorOperationType
    {
        Id = Guid.Parse("587159ea-13c2-45ff-a944-ffb30c2f3e88"),
        Name = "+",
        Type = Models.OperationType.Simple,
        InputParameters = new List<Parameter>
        {
            new Parameter { Name = "a", DataType = "numeric" },
            new Parameter { Name = "b", DataType = "numeric" }
        },
        OutputParameters = new List<Parameter>
        {
            new Parameter { Name = "result", DataType = "numeric" }
        }
    };

    public SignalProcessorOperationResult Execute(IDictionary<string, string> inputs)
    {
        if (!inputs.ContainsKey("a") || !inputs.ContainsKey("b"))
        {
            throw new ArgumentException("Inputs must contain 'a' and 'b' keys.");
        }

        if (!decimal.TryParse(inputs["a"], out var a))
        {
            throw new ArgumentException("Input 'a' is not a valid number.");
        }

        if (!decimal.TryParse(inputs["b"], out var b))
        {
            throw new ArgumentException("Input 'b' is not a valid number.");
        }

        var logs = $"Executing AddOperation with inputs: a={a}, b={b}";
        decimal result = a + b;

        return new SignalProcessorOperationResult
        {
            Outputs = new Dictionary<string, string>
            {
                { "result", result.ToString(CultureInfo.InvariantCulture) }
            },
            Logs = logs
        };
    }
}
