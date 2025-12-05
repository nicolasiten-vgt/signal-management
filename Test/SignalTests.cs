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
public class SignalTests : TestBase
{
    private static readonly SignalEntity Signal1 = new()
    {
        Id = "signal_amperage_device-001",
        Name = "Amperage",
        Input = true,
        Output = true,
        Unit = "A",
        DataType = SignalDataType.Numeric,
        Scope = SignalScope.Tenant,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test@vgt.energy"
    };
    private static readonly SignalEntity Signal2 = new()
    {
        Id = "signal_status_device-001",
        Name = "Status",
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
        dbContext.Signals.Add(Signal1);
        dbContext.Signals.Add(Signal2);
        await dbContext.SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task CreateSignal_ValidSignal_StoresSignal()
    {
        var request = new SignalCreateRequest(
            "signal_voltage_device-001",
            "Voltage",
            true,
            false,
            "V",
            SignalDataType.Numeric,
            SignalScope.Tenant,
            null);

        var response = await ApiClient.PostAsJsonAsync("/signals", request);
        
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SignalDto>(JsonSerializerOptions);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Id, created.Id);
        Assert.AreEqual(request.Name, created.Name);
        Assert.AreEqual(request.Input, created.Input);
        Assert.AreEqual(request.Output, created.Output);
        Assert.AreEqual(request.Unit, created.Unit);
        Assert.AreEqual(request.DataType, created.DataType);
        Assert.AreEqual(request.Scope, created.Scope);
        Assert.IsNull(created.EdgeInstance);
        Assert.AreEqual("test@vgt.energy", (string)created.CreatedBy);
    }

    [TestMethod]
    public async Task GetSignals_WithExistingSignals_ReturnsAllSignals()
    {
        var response = await ApiClient.GetAsync("/signals");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var signalList = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SignalDto>>(JsonSerializerOptions);
        Assert.IsNotNull(signalList);
        Assert.AreEqual(2, signalList.Count);
    }

    [TestMethod]
    public async Task GetSignals_WithExistingSignal_ReturnsSignal()
    {
        var response = await ApiClient.GetAsync($"/signals/{Signal1.Id}");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var signal = await response.Content.ReadFromJsonAsync<SignalDto>(JsonSerializerOptions);
        Assert.IsNotNull(signal);
        Assert.AreEqual(Signal1.Id, signal.Id);
    }
}
