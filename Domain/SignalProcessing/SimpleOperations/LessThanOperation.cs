namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing.SimpleOperations;

public class LessThanOperation : ISignalProcessorOperation
{
    public IDictionary<string, string> Execute(IDictionary<string, string> inputs)
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

        bool result = a < b;

        return new Dictionary<string, string>
        {
            { "result", result.ToString() }
        };
    }
}
