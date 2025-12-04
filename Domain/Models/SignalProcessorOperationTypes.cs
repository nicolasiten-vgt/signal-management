namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;

public class SignalProcessorOperationTypes
{
    public IReadOnlyCollection<SignalProcessorOperationType> All { get; }

    public SignalProcessorOperationTypes(
        IReadOnlyCollection<SignalProcessorOperationType> simpleOperationTypes,
        IReadOnlyCollection<CustomFunction> customFunctions)
    {
        var mappedFunctions = customFunctions.Select(cf => new SignalProcessorOperationType
        {
            Id = cf.Id,
            Name = cf.Name,
            Type = OperationType.CustomFunction,
            InputParameters = cf.InputParameters?.Select(p => new Parameter
            {
                Name = p.Name,
                DataType = p.DataType
            }).ToList() ?? new List<Parameter>(),
            OutputParameters = cf.OutputParameters?.Select(p => new Parameter
            {
                Name = p.Name,
                DataType = p.DataType
            }).ToList() ?? new List<Parameter>()
        });

        All = simpleOperationTypes.Concat(mappedFunctions).ToList();
    }
}

