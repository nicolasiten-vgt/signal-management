using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test;

[TestClass]
public class SignalProcessorInvokeTests : TestBase
{
    private static readonly SignalEntity NumericSignal1 = new()
    {
        Id = "signal_numeric_1",
        Name = "Numeric Signal 1",
        Input = true,
        Output = true,
        Unit = "W",
        DataType = SignalDataType.Numeric,
        Scope = SignalScope.Tenant,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test@vgt.energy"
    };

    private static readonly SignalEntity NumericSignal2 = new()
    {
        Id = "signal_numeric_2",
        Name = "Numeric Signal 2",
        Input = true,
        Output = true,
        Unit = "V",
        DataType = SignalDataType.Numeric,
        Scope = SignalScope.Tenant,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test@vgt.energy"
    };

    private static readonly SignalEntity NumericSignal3 = new()
    {
        Id = "signal_numeric_3",
        Name = "Numeric Signal 3",
        Input = true,
        Output = true,
        Unit = "A",
        DataType = SignalDataType.Numeric,
        Scope = SignalScope.Tenant,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test@vgt.energy"
    };

    private static readonly SignalEntity StringSignal = new()
    {
        Id = "signal_string_1",
        Name = "String Signal 1",
        Input = true,
        Output = true,
        Unit = "-",
        DataType = SignalDataType.String,
        Scope = SignalScope.Tenant,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test@vgt.energy"
    };

    private static readonly Guid CustomFunctionId = Guid.NewGuid();

    private static readonly CustomFunctionEntity CustomFunction = new()
    {
        Id = CustomFunctionId,
        Name = "Multiply and Add",
        Language = ProgrammingLanguage.JavaScript,
        InputParameters = new List<ParameterDefinition>
        {
            new() { Name = "x", DataType = "numeric" },
            new() { Name = "y", DataType = "numeric" }
        },
        OutputParameters = new List<ParameterDefinition>
        {
            new() { Name = "product", DataType = "numeric" },
            new() { Name = "sum", DataType = "numeric" }
        },
        SourceCode = """
                     return {
                       product: Number(inputs.x) * Number(inputs.y),
                       sum: Number(inputs.x) + Number(inputs.y)
                     };
                     """,
        Dependencies = null
    };

    [TestInitialize]
    public async Task Initialize()
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SignalDbContext>();
        dbContext.Signals.Add(NumericSignal1);
        dbContext.Signals.Add(NumericSignal2);
        dbContext.Signals.Add(NumericSignal3);
        dbContext.Signals.Add(StringSignal);
        dbContext.CustomFunctions.Add(CustomFunction);
        await dbContext.SaveChangesAsync();
    }

    [TestMethod]
    public async Task InvokeSignalProcessor_MissingSignalInput_Returns400()
    {
        // Arrange - Create processor with signal input
        var processor = await CreateSimpleAddProcessor();

        // Act - Invoke without providing required signal input
        var signalInputs = new Dictionary<string, string>();
        var response = await ApiClient.PostAsJsonAsync($"/signal-processors/{processor.Id}/invoke", signalInputs, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeSignalProcessor_WrongDataTypeForSignal_Returns400()
    {
        // Arrange - Create processor with numeric signal input
        var processor = await CreateSimpleAddProcessor();

        // Act - Invoke with string value for numeric signal
        var signalInputs = new Dictionary<string, string>
        {
            [NumericSignal1.Id] = "not-a-number",
            [NumericSignal2.Id] = "10"
        };
        var response = await ApiClient.PostAsJsonAsync($"/signal-processors/{processor.Id}/invoke", signalInputs, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeSignalProcessor_GraphWithOneSimpleOperation_ReturnsSuccessResult()
    {
        // Arrange - Create processor with single addition operation
        var processor = await CreateSimpleAddProcessor();

        // Act
        var signalInputs = new Dictionary<string, string>
        {
            [NumericSignal1.Id] = "10",
            [NumericSignal2.Id] = "20"
        };
        var response = await ApiClient.PostAsJsonAsync($"/signal-processors/{processor.Id}/invoke", signalInputs, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SignalProcessorInvokeResultDto>(JsonSerializerOptions);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StepResults);
        Assert.AreEqual(1, result.StepResults.Count);
        Assert.IsTrue(result.StepResults.ContainsKey("step1"));
        
        var stepResult = result.StepResults["step1"];
        Assert.IsInstanceOfType<StepInvocationSuccessResultDto>(stepResult);
        
        var successResult = (StepInvocationSuccessResultDto)stepResult;
        Assert.IsNotNull(successResult.OutputValues);
        Assert.IsTrue(successResult.OutputValues.ContainsKey("result"));
        Assert.AreEqual("30", successResult.OutputValues["result"]);
    }

    [TestMethod]
    public async Task InvokeSignalProcessor_GraphWithOneCustomFunction_ReturnsSuccessResult()
    {
        // Arrange - Create processor with single custom function
        var createRequest = new SignalProcessorCreateRequest(
            "Processor with Custom Function",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                new ComputeStepRequest
                {
                    Id = "step1",
                    Operation = new CustomFunctionOperationRequest
                    {
                        CustomFunctionId = CustomFunctionId
                    },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "x",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest
                            {
                                SignalId = NumericSignal1.Id
                            }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "y",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest
                            {
                                SignalId = NumericSignal2.Id
                            }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest
                        {
                            Name = "product",
                            DataType = "numeric"
                        },
                        new OutputDefinitionRequest
                        {
                            Name = "sum",
                            DataType = "numeric"
                        }
                    }
                }
            }
        );

        var createResponse = await ApiClient.PostAsJsonAsync("/signal-processors", createRequest, JsonSerializerOptions);
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
        var processor = await createResponse.Content.ReadFromJsonAsync<SignalProcessorDto>(JsonSerializerOptions);
        Assert.IsNotNull(processor);

        // Act
        var signalInputs = new Dictionary<string, string>
        {
            [NumericSignal1.Id] = "5",
            [NumericSignal2.Id] = "3"
        };
        var response = await ApiClient.PostAsJsonAsync($"/signal-processors/{processor.Id}/invoke", signalInputs, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SignalProcessorInvokeResultDto>(JsonSerializerOptions);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StepResults);
        Assert.AreEqual(1, result.StepResults.Count);
        Assert.IsTrue(result.StepResults.ContainsKey("step1"));
        
        var stepResult = result.StepResults["step1"];
        Assert.IsInstanceOfType<StepInvocationSuccessResultDto>(stepResult);
        var successResult = (StepInvocationSuccessResultDto)stepResult;
        Assert.IsNotNull(successResult.OutputValues);
        Assert.AreEqual(2, successResult.OutputValues.Count);
        Assert.IsTrue(successResult.OutputValues.ContainsKey("product"));
        Assert.IsTrue(successResult.OutputValues.ContainsKey("sum"));
        Assert.AreEqual("15", successResult.OutputValues["product"]); // 5 * 3
        Assert.AreEqual("8", successResult.OutputValues["sum"]); // 5 + 3
    }

    [TestMethod]
    public async Task InvokeSignalProcessor_ComplexGraphWithSimpleOperationsAndCustomFunction_ReturnsSuccessResult()
    {
        // Arrange - Create complex processor:
        // step1: add signal1 + signal2 -> temp1
        // step2: multiply temp1 * signal3 -> temp2
        // step3: check if temp2 > 100 -> condition
        // step4: subtract temp2 - 50 -> temp3
        // step5: custom function (multiply and add) using temp3 and constant -> final_product, final_sum
        var createRequest = new SignalProcessorCreateRequest(
            "Complex Processor",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                // Step 1: Add signal1 + signal2
                new ComputeStepRequest
                {
                    Id = "step1",
                    Operation = new SimpleOperationRequest { Action = "+" },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "a",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest { SignalId = NumericSignal1.Id }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "b",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest { SignalId = NumericSignal2.Id }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest { Name = "result", DataType = "numeric" }
                    }
                },
                // Step 2: Multiply temp1 * signal3
                new ComputeStepRequest
                {
                    Id = "step2",
                    Operation = new SimpleOperationRequest { Action = "*" },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "a",
                            DataType = "numeric",
                            Source = new StepOutputInputSourceRequest 
                            { 
                                StepId = "step1",
                                StepOutputId = "result"
                            }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "b",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest { SignalId = NumericSignal3.Id }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest { Name = "result", DataType = "numeric" }
                    }
                },
                // Step 3: Check if temp2 > 100
                new ComputeStepRequest
                {
                    Id = "step3",
                    Operation = new SimpleOperationRequest { Action = ">" },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "a",
                            DataType = "numeric",
                            Source = new StepOutputInputSourceRequest 
                            { 
                                StepId = "step2",
                                StepOutputId = "result"
                            }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "b",
                            DataType = "numeric",
                            Source = new ConstantInputSourceRequest { Value = "100" }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest { Name = "result", DataType = "boolean" }
                    }
                },
                // Step 4: Subtract temp2 - 50
                new ComputeStepRequest
                {
                    Id = "step4",
                    Operation = new SimpleOperationRequest { Action = "+" },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "a",
                            DataType = "numeric",
                            Source = new StepOutputInputSourceRequest 
                            { 
                                StepId = "step2",
                                StepOutputId = "result"
                            }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "b",
                            DataType = "numeric",
                            Source = new ConstantInputSourceRequest { Value = "-50" }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest { Name = "result", DataType = "numeric" }
                    }
                },
                // Step 5: Custom function (multiply and add) using temp3 and constant
                new ComputeStepRequest
                {
                    Id = "step5",
                    Operation = new CustomFunctionOperationRequest
                    {
                        CustomFunctionId = CustomFunctionId
                    },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "x",
                            DataType = "numeric",
                            Source = new StepOutputInputSourceRequest 
                            { 
                                StepId = "step4",
                                StepOutputId = "result"
                            }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "y",
                            DataType = "numeric",
                            Source = new ConstantInputSourceRequest { Value = "2" }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest { Name = "product", DataType = "numeric" },
                        new OutputDefinitionRequest { Name = "sum", DataType = "numeric" }
                    }
                }
            }
        );

        var createResponse = await ApiClient.PostAsJsonAsync("/signal-processors", createRequest, JsonSerializerOptions);
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
        var processor = await createResponse.Content.ReadFromJsonAsync<SignalProcessorDto>(JsonSerializerOptions);
        Assert.IsNotNull(processor);

        // Act
        var signalInputs = new Dictionary<string, string>
        {
            [NumericSignal1.Id] = "10",  // signal1
            [NumericSignal2.Id] = "20",  // signal2
            [NumericSignal3.Id] = "5"    // signal3
        };
        var response = await ApiClient.PostAsJsonAsync($"/signal-processors/{processor.Id}/invoke", signalInputs, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SignalProcessorInvokeResultDto>(JsonSerializerOptions);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.StepResults);
        Assert.AreEqual(5, result.StepResults.Count);

        // Verify step1: 10 + 20 = 30
        Assert.IsTrue(result.StepResults.ContainsKey("step1"));
        var step1Result = result.StepResults["step1"] as StepInvocationSuccessResultDto;
        Assert.IsNotNull(step1Result);
        Assert.AreEqual("30", step1Result.OutputValues["result"]);

        // Verify step2: 30 * 5 = 150
        Assert.IsTrue(result.StepResults.ContainsKey("step2"));
        var step2Result = result.StepResults["step2"] as StepInvocationSuccessResultDto;
        Assert.IsNotNull(step2Result);
        Assert.AreEqual("150", step2Result.OutputValues["result"]);

        // Verify step3: 150 > 100 = true
        Assert.IsTrue(result.StepResults.ContainsKey("step3"));
        var step3Result = result.StepResults["step3"] as StepInvocationSuccessResultDto;
        Assert.IsNotNull(step3Result);
        Assert.AreEqual("True", step3Result.OutputValues["result"]);

        // Verify step4: 150 - 50 = 100
        Assert.IsTrue(result.StepResults.ContainsKey("step4"));
        var step4Result = result.StepResults["step4"] as StepInvocationSuccessResultDto;
        Assert.IsNotNull(step4Result);
        Assert.AreEqual("100", step4Result.OutputValues["result"]);

        // Verify step5: custom function (100 * 2 = 200, 100 + 2 = 102)
        Assert.IsTrue(result.StepResults.ContainsKey("step5"));
        var step5Result = result.StepResults["step5"] as StepInvocationSuccessResultDto;
        Assert.IsNotNull(step5Result);
        Assert.AreEqual("200", step5Result.OutputValues["product"]);
        Assert.AreEqual("102", step5Result.OutputValues["sum"]);
    }

    private async Task<SignalProcessorDto> CreateSimpleAddProcessor()
    {
        var createRequest = new SignalProcessorCreateRequest(
            "Simple Add Processor",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                new ComputeStepRequest
                {
                    Id = "step1",
                    Operation = new SimpleOperationRequest
                    {
                        Action = "+"
                    },
                    Inputs = new List<InputDefinitionRequest>
                    {
                        new InputDefinitionRequest
                        {
                            Name = "a",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest
                            {
                                SignalId = NumericSignal1.Id
                            }
                        },
                        new InputDefinitionRequest
                        {
                            Name = "b",
                            DataType = "numeric",
                            Source = new SignalInputSourceRequest
                            {
                                SignalId = NumericSignal2.Id
                            }
                        }
                    },
                    Outputs = new List<OutputDefinitionRequest>
                    {
                        new OutputDefinitionRequest
                        {
                            Name = "result",
                            DataType = "numeric"
                        }
                    }
                }
            }
        );

        var response = await ApiClient.PostAsJsonAsync("/signal-processors", createRequest, JsonSerializerOptions);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var processor = await response.Content.ReadFromJsonAsync<SignalProcessorDto>(JsonSerializerOptions);
        Assert.IsNotNull(processor);
        return processor;
    }
}
