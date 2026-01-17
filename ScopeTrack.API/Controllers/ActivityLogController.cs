using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ActivityLogController(IActivityLogService service) : ControllerBase
  {
    private readonly IActivityLogService _service = service;

    [HttpGet("{entityId}")]
    public async Task<IActionResult> GetByEntityIDAsync(
      Guid entityId,
      CancellationToken ct
    )
    {
      RequestResult<IReadOnlyList<ActivityLogGetDTO>> RequestResult =
        await _service.GetByEntityIDAsync(entityId, ct);

      return Ok(RequestResult.Value);
    }
  }
}
