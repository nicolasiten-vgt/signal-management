using System.Globalization;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

public class LessThanOperation : ISignalProcessorSimpleOperation
{
    public SignalProcessorOperationType OperationType { get; } = new SignalProcessorOperationType
    {
        Id = Guid.Parse("4e93560e-0185-4b66-a1a4-86a8634a49ea"),
        Name = "<",
        Type = Models.OperationType.Simple,
        InputParameters = new List<Parameter>
        {
            new Parameter { Name = "a", DataType = "numeric" },
            new Parameter { Name = "b", DataType = "numeric" }
        },
        OutputParameters = new List<Parameter>
        {
            new Parameter { Name = "result", DataType = "string" }
        }
    };

    public SignalProcessorOperationResult Execute(IDictionary<string, string> inputs)
    {
        if (!inputs.ContainsKey("a") || !inputs.ContainsKey("b"))
        {
            throw new ArgumentException("Inputs must contain 'a' and 'b' keys.");
        }

        if (!decimal.TryParse(inputs["a"], out decimal a))
        {
            throw new ArgumentException("Input 'a' is not a valid number.");
        }

        if (!decimal.TryParse(inputs["b"], out decimal b))
        {
            throw new ArgumentException("Input 'b' is not a valid number.");
        }

        var logs = $"Executing LessThanOperation with inputs: a={a}, b={b}";
        bool result = a < b;

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

