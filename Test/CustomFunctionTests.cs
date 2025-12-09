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
public class CustomFunctionTests : TestBase
{
    private static readonly Guid CustomFunction1Id = Guid.NewGuid();
    private static readonly Guid CustomFunction2Id = Guid.NewGuid();
    
    private static readonly CustomFunctionEntity CustomFunction1 = new()
    {
        Id = CustomFunction1Id,
        Name = "Add Two Numbers",
        Language = ProgrammingLanguage.JavaScript,
        InputParameters = new List<ParameterDefinition>
        {
            new() { Name = "a", DataType = "numeric" },
            new() { Name = "b", DataType = "numeric" }
        },
        OutputParameters = new List<ParameterDefinition>
        {
            new() { Name = "result", DataType = "numeric" }
        },
        SourceCode = "return { result: Number(inputs.a) + Number(inputs.b) };",
        Dependencies = null
    };
    
    private static readonly CustomFunctionEntity CustomFunction2 = new()
    {
        Id = CustomFunction2Id,
        Name = "Concatenate Strings",
        Language = ProgrammingLanguage.JavaScript,
        InputParameters = new List<ParameterDefinition>
        {
            new() { Name = "str1", DataType = "string" },
            new() { Name = "str2", DataType = "string" }
        },
        OutputParameters = new List<ParameterDefinition>
        {
            new() { Name = "result", DataType = "string" }
        },
        SourceCode = "return { result: inputs.str1 + inputs.str2 };",
        Dependencies = null
    };
    
    [TestInitialize]
    public async Task Initialize()
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SignalDbContext>();
        dbContext.CustomFunctions.Add(CustomFunction1);
        dbContext.CustomFunctions.Add(CustomFunction2);
        await dbContext.SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task CreateCustomFunction_ValidRequest_StoresCustomFunction()
    {
        var request = new CustomFunctionCreateRequest(
            "Multiply Two Numbers",
            ProgrammingLanguage.JavaScript,
            new List<ParameterDefinition>
            {
                new() { Name = "x", DataType = "numeric" },
                new() { Name = "y", DataType = "numeric" }
            },
            new List<ParameterDefinition>
            {
                new() { Name = "product", DataType = "numeric" }
            },
            "return { product: Number(inputs.x) * Number(inputs.y) };",
            null);

        var response = await ApiClient.PostAsJsonAsync("/custom-functions", request);
        
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CustomFunctionDto>(JsonSerializerOptions);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Name, created.Name);
        Assert.AreEqual(request.Language, created.Language);
        Assert.AreEqual(request.SourceCode, created.SourceCode);
        Assert.IsNotNull(created.InputParameters);
        Assert.AreEqual(2, created.InputParameters.Count);
    }

    [TestMethod]
    public async Task GetCustomFunctions_WithExistingCustomFunctions_ReturnsAllCustomFunctions()
    {
        var response = await ApiClient.GetAsync("/custom-functions");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var customFunctionList = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<CustomFunctionDto>>(JsonSerializerOptions);
        Assert.IsNotNull(customFunctionList);
        Assert.AreEqual(2, customFunctionList.Count);
    }

    [TestMethod]
    public async Task GetCustomFunction_WithExistingCustomFunction_ReturnsCustomFunction()
    {
        var response = await ApiClient.GetAsync($"/custom-functions/{CustomFunction1Id}");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var customFunction = await response.Content.ReadFromJsonAsync<CustomFunctionDto>(JsonSerializerOptions);
        Assert.IsNotNull(customFunction);
        Assert.AreEqual(CustomFunction1Id, customFunction.Id);
        Assert.AreEqual(CustomFunction1.Name, customFunction.Name);
    }

    [TestMethod]
    public async Task GetCustomFunction_WithNonExistingCustomFunction_ReturnsNotFound()
    {
        var nonExistingId = Guid.NewGuid();
        var response = await ApiClient.GetAsync($"/custom-functions/{nonExistingId}");
        
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteCustomFunction_WithExistingCustomFunction_ReturnsNoContent()
    {
        var response = await ApiClient.DeleteAsync($"/custom-functions/{CustomFunction1Id}");
        
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify deletion
        var getResponse = await ApiClient.GetAsync($"/custom-functions/{CustomFunction1Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task DeleteCustomFunction_WithNonExistingCustomFunction_ReturnsNotFound()
    {
        var nonExistingId = Guid.NewGuid();
        var response = await ApiClient.DeleteAsync($"/custom-functions/{nonExistingId}");
        
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
