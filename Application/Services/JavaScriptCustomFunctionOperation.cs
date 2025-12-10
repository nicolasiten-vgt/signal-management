using System.Text.Json;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

public class JavaScriptCustomFunctionOperation : ISignalProcessorOperation
{
    private readonly string _sourceCode;

    public JavaScriptCustomFunctionOperation(string sourceCode)
    {
        _sourceCode = sourceCode;
    }

    public SignalProcessorOperationResult Execute(IDictionary<string, string> inputs)
    {
        using var engine = CreateEngine(inputs, out var console);
        engine.Script.jsonInput = JsonSerializer.Serialize(inputs);
        var jsResult = engine.Evaluate($$"""
         (function (json) {
            const inputs = JSON.parse(json);
            {{_sourceCode}}
         })(jsonInput)
         """);
        
        if (jsResult is ScriptObject scriptObject)
        {
            var result = scriptObject.PropertyNames
                .ToDictionary(name => name, name => scriptObject.GetProperty(name)?.ToString() ?? string.Empty);
            
            return new SignalProcessorOperationResult
            {
                Outputs = result,
                Logs = string.Join(Environment.NewLine, console.Logs)
            };
        }
        
        throw new InvalidOperationException("Custom function did not return a valid object");
    }
    
    private static V8ScriptEngine CreateEngine(
        IDictionary<string, string> inputs,
        out JsConsole jsConsole)
    {
        var engine = new V8ScriptEngine();

        engine.Script.jsonInput = JsonSerializer.Serialize(inputs);

        jsConsole = new JsConsole();
        engine.AddHostObject("csConsole", HostItemFlags.PrivateAccess, jsConsole);

        engine.Execute(@"
            var console = {
                log: function () {
                    var parts = [];

                    for (var i = 0; i < arguments.length; i++) {
                        var arg = arguments[i];

                        if (arg && typeof arg === 'object') {
                            parts.push(JSON.stringify(arg));
                        } else {
                            parts.push(String(arg));
                        }
                    }

                    csConsole.Log(parts.join(' '));
                }
            };
        ");

        return engine;
    }    
    
    private class JsConsole
    {
        private readonly List<string> _logs = new();
        public IReadOnlyCollection<string> Logs => _logs;

        public void Log(params object[] args)
        {
            _logs.Add(string.Join(" ", args.Select(a => a?.ToString())));
        }
    }    
}
