using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.API.Controllers
{
  [ApiController]
  [Route("api/activity-logs")]
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
      if (!TryMapEntityType(entityType, out ActivityEntityType type))
        return BadRequest("Invalid entity type");

      bool exists
        = await _activityLogService.EntityExistsAsync(type, entityId, ct);
      if (!exists)
        return NotFound("Entity not found");

      IReadOnlyList<ActivityLogGetDto> result
        = await _activityLogService.GetByEntityAsync(type, entityId, ct);

      return Ok(result);
    }

    private static bool TryMapEntityType(
      string value,
      out ActivityEntityType entityType
    )
    {
      entityType = value.ToLowerInvariant() switch
      {
        "clients" => ActivityEntityType.Client,
        "contracts" => ActivityEntityType.Contract,
        "deliverables" => ActivityEntityType.Deliverable,
        _ => default
      };

      return entityType != default;
    }
  }
}
