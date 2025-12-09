using System.Text.Json;
using Microsoft.ClearScript.V8;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class JavaScriptCustomFunctionOperation : Domain.SignalProcessing.ISignalProcessorOperation
{
    private readonly string _sourceCode;

    public JavaScriptCustomFunctionOperation(string sourceCode)
    {
        _sourceCode = sourceCode;
    }

    public IDictionary<string, string> Execute(IDictionary<string, string> inputs)
    {
        using var engine = new V8ScriptEngine();
        engine.Execute(_sourceCode);
        
        string input = JsonSerializer.Serialize(inputs);
        var jsResult = engine.Script.run(input);
        
        if (jsResult is Microsoft.ClearScript.ScriptObject scriptObject)
        {
            var result = scriptObject.PropertyNames
                .ToDictionary(name => name, name => scriptObject.GetProperty(name)?.ToString() ?? string.Empty);
            return result;
        }
        
        throw new InvalidOperationException("Custom function did not return a valid object");
    }
}
