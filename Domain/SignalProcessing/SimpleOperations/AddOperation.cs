using System.Globalization;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

public class AddOperation : ISignalProcessorOperation
{
    public IDictionary<string, string> Execute(IDictionary<string, string> inputs)
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

        decimal result = a + b;

        return new Dictionary<string, string>
        {
            { "result", result.ToString(CultureInfo.InvariantCulture) }
        };
    }
}