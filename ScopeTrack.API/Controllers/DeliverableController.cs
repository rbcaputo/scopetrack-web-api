using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DeliverableController(
    IValidator<DeliverablePatchDTO> patchValidator,
    IDeliverableService service) : ControllerBase
  {
    private readonly IValidator<DeliverablePatchDTO> _patchValidator
      = patchValidator;
    private readonly IDeliverableService _service = service;

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatusAsync(
      Guid id,
      DeliverablePatchDTO dto,
      CancellationToken ct
    )
    {
      ValidationResult validationResult =
        await _patchValidator.ValidateAsync(dto, ct);
      if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => new
        {
          field = e.PropertyName,
          message = e.ErrorMessage
        }));

      RequestResult<DeliverableGetDTO> requestResult
        = await _service.UpdateStatusAsync(id, dto, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<DeliverableGetDTO> requestResult
        = await _service.GetByIDAsync(id, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }
  }
}
