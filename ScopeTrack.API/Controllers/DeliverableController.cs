using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [ApiController]
  [Route("api/deliverables")]
  public class DeliverableController(
    IDeliverableService service,
    IValidator<DeliverablePatchDto> deliverablePatchValidator
  ) : ControllerBase
  {
    private readonly IDeliverableService _service = service;
    private readonly IValidator<DeliverablePatchDto> _deliverablePatchValidator
      = deliverablePatchValidator;

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatusAsync(
      Guid id,
      [FromBody] DeliverablePatchDto dto,
      CancellationToken ct
    )
    {
      ValidationResult validation
        = await _deliverablePatchValidator.ValidateAsync(dto, ct);
      if (!validation.IsValid)
        return BadRequest(new
        {
          errors = validation.Errors.Select(er => er.ErrorMessage)
        });

      RequestResult<DeliverableGetDto> result
        = await _service.UpdateStatusAsync(id, dto, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpGet("{id}", Name = "GetDeliverableById")]
    public async Task<IActionResult> GetByIdAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<DeliverableGetDto> result
        = await _service.GetByIdAsync(id, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }
  }
}
