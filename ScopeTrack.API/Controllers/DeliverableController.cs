using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DeliverableController(IDeliverableService service) : ControllerBase
  {
    private readonly IDeliverableService _service = service;

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatusAsync(
      Guid id,
      DeliverablePatchDTO dto,
      CancellationToken ct
    )
    {
      Result<DeliverableGetDTO> result = await _service.UpdateStatusAsync(id, dto, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      Result<DeliverableGetDTO> result = await _service.GetByIDAsync(id, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }
  }
}
