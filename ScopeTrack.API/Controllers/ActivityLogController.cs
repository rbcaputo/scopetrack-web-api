using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ActivityLogController(IActivityLogService activityLogService) : ControllerBase
  {
    private readonly IActivityLogService _activityLogService
      = activityLogService;

    [HttpGet("{entityType}/{entityId}")]
    public async Task<IActionResult> GetByEntityIdAsync(
      string entityType,
      Guid entityId,
      CancellationToken ct
    )
    {
      if (!Enum.TryParse(entityType, out ActivityEntityType type))
        return BadRequest("Invalid entity type");

      bool exists
        = await _activityLogService.EntityExistsAsync(type, entityId, ct);
      if (!exists)
        return NotFound("Entity not found");

      IReadOnlyList<ActivityLogGetDto> result
        = await _activityLogService.GetByEntityAsync(type, entityId, ct);

      return Ok(result);
    }
  }
}
