using Microsoft.AspNetCore.Mvc;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Controllers;

[ApiController]
[Route("signals")]
[Produces("application/json")]
public class SignalController : ControllerBase
{
    private readonly ISignalService _service;

    public SignalController(ISignalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSignalsAsync(CancellationToken cancellationToken)
    {
        var signals = await _service.GetAllAsync(cancellationToken);
        return Ok(signals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSignalByIdAsync(string id, CancellationToken cancellationToken)
    {
        var signal = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(signal);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSignalAsync([FromBody] SignalCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return Created($"/signals/{created.Id}", created);
    }
}