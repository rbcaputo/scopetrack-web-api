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
  public class ContractController(
    IValidator<ContractPatchDTO> patchValidator,
    IValidator<DeliverablePostDTO> deliverablePostValidator,
    IContractService service
  ) : ControllerBase
  {
    private readonly IValidator<ContractPatchDTO> _patchValidator
      = patchValidator;
    private readonly IValidator<DeliverablePostDTO> _deliverablePostValidator
      = deliverablePostValidator;
    private readonly IContractService _service = service;

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatusAsync(
      Guid id,
      [FromBody] ContractPatchDTO dto,
      CancellationToken ct
    )
    {
      ValidationResult validationResult
        = await _patchValidator.ValidateAsync(dto, ct);
      if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(er => new
        {
          field = er.PropertyName,
          message = er.ErrorMessage
        }));

      RequestResult<ContractGetDTO> requestResult
        = await _service.UpdateStatusAsync(id, dto, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }

    [HttpPost("{id}/deliverables")]
    public async Task<IActionResult> AddDeliverableAsync(
      Guid id,
      [FromBody] DeliverablePostDTO dto,
      CancellationToken ct
    )
    {
      ValidationResult validationResult
        = await _deliverablePostValidator.ValidateAsync(dto, ct);
      if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(er => new
        {
          field = er.PropertyName,
          message = er.ErrorMessage
        }));

      RequestResult<DeliverableGetDTO> requestResult
        = await _service.AddDeliverableAsync(id, dto, ct);

      return requestResult.IsSuccess
        ? CreatedAtAction(
          nameof(GetByIDAsync),
          "Deliverable",
          new { id = requestResult.Value!.ID },
          requestResult.Value
        )
        : NotFound(requestResult.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<ContractGetDTO> requestResult
        = await _service.GetByIDAsync(id, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }
  }
}
