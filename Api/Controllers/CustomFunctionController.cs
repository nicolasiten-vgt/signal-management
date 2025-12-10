using Microsoft.AspNetCore.Mvc;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Dtos;
using VGT.Galaxy.Backend.Services.SignalManagement.Api.Mappings;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Requests;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Api.Controllers;

[ApiController]
[Route("custom-functions")]
[Produces("application/json")]
public class CustomFunctionController : ControllerBase
{
    private readonly ICustomFunctionService _service;

    public CustomFunctionController(ICustomFunctionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CustomFunctionDto>>> GetCustomFunctionsAsync(CancellationToken cancellationToken)
    {
        var customFunctions = await _service.GetAllAsync(cancellationToken);
        var customFunctionDtos = customFunctions.ToDto();
        return Ok(customFunctionDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomFunctionDto>> GetCustomFunctionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customFunction = await _service.GetByIdAsync(id, cancellationToken);
        var customFunctionDto = customFunction.ToDto();
        return Ok(customFunctionDto);
    }

    [HttpPost]
    public async Task<ActionResult<CustomFunctionDto>> CreateCustomFunctionAsync([FromBody] CustomFunctionCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        var createdDto = created.ToDto();
        return Created($"/custom-functions/{createdDto.Id}", createdDto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomFunctionAsync(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
