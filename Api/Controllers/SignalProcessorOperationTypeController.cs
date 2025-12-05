using Microsoft.AspNetCore.Mvc;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Mappings;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Controllers;

[ApiController]
[Route("signal-processor-operation-types")]
[Produces("application/json")]
public class SignalProcessorOperationTypeController : ControllerBase
{
    private readonly ISignalProcessorOperationTypeService _service;

    public SignalProcessorOperationTypeController(ISignalProcessorOperationTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var operationTypes = await _service.GetAllAsync(cancellationToken);
        var operationTypeDtos = operationTypes.ToDto();
        return Ok(operationTypeDtos);
    }
}
