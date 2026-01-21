using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ContractController(IContractService service) : ControllerBase
  {
    private readonly IContractService _service = service;

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatusAsync(
      Guid id,
      [FromBody] ContractPatchDto dto,
      CancellationToken ct
    )
    {
      RequestResult<ContractGetDto> result
        = await _service.UpdateStatusAsync(id, dto, ct);

      if (!result.IsSuccess && result.Error == "Invalid contract status")
        return BadRequest(new { errors = new[] { result.Error } });

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpPost("{id}/deliverables")]
    public async Task<IActionResult> AddDeliverableAsync(
      Guid id,
      [FromBody] DeliverablePostDto dto,
      CancellationToken ct
    )
    {
      RequestResult<DeliverableGetDto> result
        = await _service.AddDeliverableAsync(id, dto, ct);

      return result.IsSuccess
        ? CreatedAtRoute(
            "GetDeliverableById",
            new { id = result.Value!.Id },
            result.Value
        )
        : NotFound(result.Error);
    }

    [HttpGet("{id}", Name = "GetContractById")]
    public async Task<IActionResult> GetByIdAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<ContractGetDto> result
        = await _service.GetByIdAsync(id, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
      => Ok(await _service.GetAllAsync(ct));
  }
}
