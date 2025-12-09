using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Models;
using VGT.Galaxy.Backend.Services.SignalManagement.Test.Helpers;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test;

[TestClass]
public class SignalProcessorTests : TestBase
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

    [TestInitialize]
    public async Task Initialize()
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SignalDbContext>();
        dbContext.Signals.Add(NumericSignal1);
        dbContext.Signals.Add(NumericSignal2);
        dbContext.Signals.Add(StringSignal);
        await dbContext.SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task CreateSignalProcessor_GraphWithCycle_Returns400()
    {
        // Arrange - Create a graph with a cycle: step1 -> step2 -> step1
        var request = new SignalProcessorCreateRequest(
            "Processor with Cycle",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                        SignalProcessorInputDefinitionFactory.CreateStepOutputInput("b", "numeric", "step2", "result")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ]),                
                SignalProcessorComputeStepFactory.CreateSimpleStep("step2", "*",
                    [
                        SignalProcessorInputDefinitionFactory.CreateStepOutputInput("a", "numeric", "step1", "result"),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "2")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ])
            }
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync("/signal-processors", request, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("ComputeGraph"));
        Assert.IsTrue(errorResponse.Errors["ComputeGraph"].Any(e => e.Contains("acyclic") || e.Contains("cycle")));
    }


    [TestMethod]
    public async Task CreateSignalProcessor_GraphNotConnected_Returns400()
    {
        // Arrange - Create two disconnected components
        var request = new SignalProcessorCreateRequest(
            "Processor with Disconnected Graph",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "10")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ]),
                SignalProcessorComputeStepFactory.CreateSimpleStep("step2", "*",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal2.Id),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "5")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ])
            }
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync("/signal-processors", request, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("ComputeGraph"));
        Assert.IsTrue(errorResponse.Errors["ComputeGraph"].Any(e => e.Contains("connected")));
    }

    [TestMethod]
    public async Task CreateSignalProcessor_DuplicateStepIds_Returns400()
    {
        // Arrange - two steps with the same Id "step1"
        var request = new SignalProcessorCreateRequest(
            "Processor with Duplicate Step IDs",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("b", "numeric", NumericSignal2.Id)
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ]),
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "*",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "2")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ])
            }
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync("/signal-processors", request, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("ComputeGraph"));
        Assert.IsTrue(errorResponse.Errors["ComputeGraph"]
            .Any(e => e.Contains("unique", StringComparison.OrdinalIgnoreCase)
                   || e.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task CreateSignalProcessor_DuplicateName_Returns400()
    {
        // Arrange - Create first processor
        var validRequest = CreateValidSignalProcessorRequest("Duplicate Name Processor");
        var response1 = await ApiClient.PostAsJsonAsync("/signal-processors", validRequest, JsonSerializerOptions);
        Assert.AreEqual(HttpStatusCode.Created, response1.StatusCode);
        
        // Act - Try to create another with the same name
        var duplicateRequest = CreateValidSignalProcessorRequest("Duplicate Name Processor");
        var response2 = await ApiClient.PostAsJsonAsync("/signal-processors", duplicateRequest, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);
        var errorResponse = await response2.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("Name"));
        Assert.IsTrue(errorResponse.Errors["Name"].Any(e => e.Contains("already exists")));
    }

    [TestMethod]
    public async Task CreateSignalProcessor_NonExistingSignalId_Returns400()
    {
        // Arrange
        var request = new SignalProcessorCreateRequest(
            "Processor with Invalid Signal",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", "non-existent-signal-id"),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "10")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ])
            }
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync("/signal-processors", request, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("ComputeGraph"));
        Assert.IsTrue(errorResponse.Errors["ComputeGraph"]
            .Any(e => e.Contains("non-existent") || e.Contains("does not exist")));
    }

    [TestMethod]
    public async Task CreateSignalProcessor_NonExistingStepOutputReference_Returns400()
    {
        // Arrange
        var request = new SignalProcessorCreateRequest(
            "Processor with Invalid Step Output",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "10")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ]),                
                SignalProcessorComputeStepFactory.CreateSimpleStep("step2", "*",
                    [
                        SignalProcessorInputDefinitionFactory.CreateStepOutputInput("a", "numeric", "step1", "non_existent_output"), // invalid output reference
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "2")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ])
            }
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync("/signal-processors", request, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("ComputeGraph"));
        Assert.IsTrue(errorResponse.Errors["ComputeGraph"]
            .Any(e => e.Contains("output") && e.Contains("does not exist")));
    }

    [TestMethod]
    public async Task CreateSignalProcessor_DataTypeMismatch_Returns400()
    {
        // Arrange - Connect numeric output to string input
        var request = new SignalProcessorCreateRequest(
            "Processor with Data Type Mismatch",
            RecomputeTrigger.SignalChange,
            null,
            new List<ComputeStepRequest>
            {
                SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                    [
                        SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "10")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ]),        
                SignalProcessorComputeStepFactory.CreateSimpleStep("step2", ">",
                    [
                        SignalProcessorInputDefinitionFactory.CreateStepOutputInput("a", "string", "step1", "result"), // wrong datatype
                        SignalProcessorInputDefinitionFactory.CreateConstantInput("b", "numeric", "100")
                    ],
                    [
                        SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                    ]),            
            }
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync("/signal-processors", request, JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonSerializerOptions);
        Assert.IsNotNull(errorResponse);
        Assert.IsTrue(errorResponse.Errors.ContainsKey("ComputeGraph"));
        Assert.IsTrue(errorResponse.Errors["ComputeGraph"].Any(e => e.Contains("mismatch") || e.Contains("type")));
    }

    [TestMethod]
    public async Task GetAllSignalProcessors_WithExistingProcessors_ReturnsProcessors()
    {
        // Arrange - Create a processor first
        var createRequest = CreateValidSignalProcessorRequest("Test Processor for GetAll");
        await ApiClient.PostAsJsonAsync("/signal-processors", createRequest, JsonSerializerOptions);

        // Act
        var response = await ApiClient.GetAsync("/signal-processors");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var processors =
            await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SignalProcessorDto>>(JsonSerializerOptions);
        Assert.IsNotNull(processors);
        Assert.IsTrue(processors.Count > 0);
        Assert.IsTrue(processors.Any(p => p.Name == "Test Processor for GetAll"));
    }

    [TestMethod]
    public async Task GetSignalProcessorById_WithExistingProcessor_ReturnsProcessor()
    {
        // Arrange - Create a processor first
        var createRequest = CreateValidSignalProcessorRequest("Test Processor for GetById");
        var createResponse =
            await ApiClient.PostAsJsonAsync("/signal-processors", createRequest, JsonSerializerOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<SignalProcessorDto>(JsonSerializerOptions);
        Assert.IsNotNull(created);

        // Act
        var response = await ApiClient.GetAsync($"/signal-processors/{created.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var processor = await response.Content.ReadFromJsonAsync<SignalProcessorDto>(JsonSerializerOptions);
        Assert.IsNotNull(processor);
        Assert.AreEqual(created.Id, processor.Id);
        Assert.AreEqual(created.Name, processor.Name);
    }

    [TestMethod]
    public async Task DeleteSignalProcessor_WithExistingProcessor_ReturnsNoContent()
    {
        // Arrange - Create a processor first
        var createRequest = CreateValidSignalProcessorRequest("Test Processor for Delete");
        var createResponse =
            await ApiClient.PostAsJsonAsync("/signal-processors", createRequest, JsonSerializerOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<SignalProcessorDto>(JsonSerializerOptions);
        Assert.IsNotNull(created);

        // Act
        var response = await ApiClient.DeleteAsync($"/signal-processors/{created.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's actually deleted
        var getResponse = await ApiClient.GetAsync($"/signal-processors/{created.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static SignalProcessorCreateRequest CreateValidSignalProcessorRequest(string name)
    {
        return new SignalProcessorCreateRequest(
            name,
            RecomputeTrigger.SignalChange,
            null,
            [SignalProcessorComputeStepFactory.CreateSimpleStep("step1", "+",
                [
                    SignalProcessorInputDefinitionFactory.CreateSignalInput("a", "numeric", NumericSignal1.Id),
                    SignalProcessorInputDefinitionFactory.CreateSignalInput("b", "numeric", NumericSignal2.Id)
                ],
                [
                    SignalProcessorOutputDefinitionFactory.CreateOutput("result", "numeric")
                ])]
        );
    }

    private class ValidationErrorResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}

