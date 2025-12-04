using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetCustomFunctionsAsync(CancellationToken cancellationToken)
    {
        var customFunctions = await _service.GetAllAsync(cancellationToken);
        return Ok(customFunctions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomFunctionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customFunction = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(customFunction);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomFunctionAsync([FromBody] CustomFunctionCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return Created($"/custom-functions/{created.Id}", created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomFunctionAsync(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
