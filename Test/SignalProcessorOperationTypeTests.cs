using Microsoft.Extensions.DependencyInjection;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test;

[TestClass]
public class SignalProcessorOperationTypeTests : TestBase
{
    private IServiceScope _scope = null!;
    private ISignalProcessorOperationTypeService _operationTypeService = null!;
    private ICustomFunctionService _customFunctionService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _scope = WebApplicationFactory.Services.CreateScope();
        _operationTypeService = _scope.ServiceProvider.GetRequiredService<ISignalProcessorOperationTypeService>();
        _customFunctionService = _scope.ServiceProvider.GetRequiredService<ICustomFunctionService>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _scope.Dispose();
    }
    
    [TestMethod]
    public async Task GetAllAsync_WithoutCustomFunctions_ReturnsSimpleOperations()
    {
        // Act
        List<SignalProcessorOperationType> result = await _operationTypeService.GetAllAsync();

        // Assert - Should have at least 4 simple operations
        List<SignalProcessorOperationType> simpleOps = result.Where(x => x.Type == OperationType.Simple).ToList();
        Assert.AreEqual(4, simpleOps.Count);
        
        SignalProcessorOperationType? addOp = simpleOps.FirstOrDefault(x => x.Name == "+");
        Assert.IsNotNull(addOp);
        Assert.AreEqual(OperationType.Simple, addOp.Type);
        Assert.AreEqual(2, addOp.InputParameters.Count);
        Assert.AreEqual(1, addOp.OutputParameters.Count);

        SignalProcessorOperationType? multiplyOp = simpleOps.FirstOrDefault(x => x.Name == "*");
        Assert.IsNotNull(multiplyOp);

        SignalProcessorOperationType? greaterOp = simpleOps.FirstOrDefault(x => x.Name == ">");
        Assert.IsNotNull(greaterOp);
        Assert.AreEqual(">", greaterOp.Name);

        SignalProcessorOperationType? lessOp = simpleOps.FirstOrDefault(x => x.Name == "<");
        Assert.IsNotNull(lessOp);
        Assert.AreEqual("<", lessOp.Name);
    }

    [TestMethod]
    public async Task GetAllAsync_WithCustomFunctions_ReturnsSimpleOperationsAndCustomFunctions()
    {
        // Arrange - Create a custom function
        CustomFunctionCreateRequest createRequest = new(
            Name: "TestCustomFunction",
            Language: ProgrammingLanguage.Csharp,
            InputParameters: new List<ParameterDefinition>
            {
                new() { Name = "a", DataType = "numeric" }
            },
            OutputParameters: new List<ParameterDefinition>
            {
                new() { Name = "result", DataType = "numeric" }
            },
            SourceCode: "return a + b;",
            Dependencies: null
        );
        CustomFunction customFunction = await _customFunctionService.CreateAsync(createRequest, CancellationToken.None);

        // Act
        List<SignalProcessorOperationType> result = await _operationTypeService.GetAllAsync();

        // Assert - Should include the custom function
        Assert.IsTrue(result.Count > 1); // custom function plus simple operations
        SignalProcessorOperationType? customFunctionOp = result.FirstOrDefault(x => x.Id == customFunction.Id);
        Assert.IsNotNull(customFunctionOp);
        Assert.AreEqual("TestCustomFunction", customFunctionOp.Name);
        Assert.AreEqual(OperationType.CustomFunction, customFunctionOp.Type);
        Assert.AreEqual(1, customFunctionOp.InputParameters.Count);
        Assert.AreEqual("a", customFunctionOp.InputParameters[0].Name);
        Assert.AreEqual("numeric", customFunctionOp.InputParameters[0].DataType);
        Assert.AreEqual(1, customFunctionOp.OutputParameters.Count);
        Assert.AreEqual("result", customFunctionOp.OutputParameters[0].Name);
    }
}
