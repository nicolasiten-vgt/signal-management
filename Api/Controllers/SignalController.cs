using Microsoft.AspNetCore.Mvc;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Mappings;
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
    public async Task<ActionResult<IReadOnlyCollection<SignalDto>>> GetSignalsAsync(CancellationToken cancellationToken)
    {
        var signals = await _service.GetAllAsync(cancellationToken);
        var signalDtos = signals.ToDto();
        return Ok(signalDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SignalDto>> GetSignalByIdAsync(string id, CancellationToken cancellationToken)
    {
        var signal = await _service.GetByIdAsync(id, cancellationToken);
        var signalDto = signal.ToDto();
        return Ok(signalDto);
    }

    [HttpPost]
    public async Task<ActionResult<SignalDto>> CreateSignalAsync([FromBody] SignalCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        var createdDto = created.ToDto();
        return Created($"/signals/{createdDto.Id}", createdDto);
    }
}