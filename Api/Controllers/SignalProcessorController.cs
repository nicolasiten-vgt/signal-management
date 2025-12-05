using Microsoft.AspNetCore.Mvc;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Mappings;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Controllers;

[ApiController]
[Route("signal-processors")]
[Produces("application/json")]
public class SignalProcessorController : ControllerBase
{
    private readonly ISignalProcessorService _service;

    public SignalProcessorController(ISignalProcessorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSignalProcessorsAsync(CancellationToken cancellationToken)
    {
        var signalProcessors = await _service.GetAllAsync(cancellationToken);
        var signalProcessorDtos = signalProcessors.ToDto();
        return Ok(signalProcessorDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSignalProcessorByIdAsync(string id, CancellationToken cancellationToken)
    {
        var signalProcessor = await _service.GetByIdAsync(id, cancellationToken);
        var signalProcessorDto = signalProcessor.ToDto();
        return Ok(signalProcessorDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSignalProcessorAsync([FromBody] SignalProcessorCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        var createdDto = created.ToDto();
        return Created($"/signal-processors/{createdDto.Id}", createdDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSignalProcessorAsync(string id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
