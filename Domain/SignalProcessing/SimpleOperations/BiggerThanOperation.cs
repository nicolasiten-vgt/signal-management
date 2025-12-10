using System.Globalization;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

public class BiggerThanOperation : ISignalProcessorSimpleOperation
{
    public SignalProcessorOperationType OperationType { get; } = new SignalProcessorOperationType
    {
        Id = Guid.Parse("80c1f252-39f1-4700-9671-2eb229b706b9"),
        Name = ">",
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

        var logs = $"Executing BiggerThanOperation with inputs: a={a}, b={b}";
        bool result = a > b;

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

