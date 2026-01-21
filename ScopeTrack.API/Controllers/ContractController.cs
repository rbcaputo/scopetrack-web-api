using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ContractController(
    IContractService service,
    IValidator<ContractPatchDto> contractPatchValidator,
    IValidator<DeliverablePostDto> deliverablePostValidator
  ) : ControllerBase
  {
    private readonly IContractService _service = service;
    private readonly IValidator<ContractPatchDto> _contractPatchValidator
      = contractPatchValidator;
    private readonly IValidator<DeliverablePostDto> _deliverablePostValidator
      = deliverablePostValidator;

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatusAsync(
      Guid id,
      [FromBody] ContractPatchDto dto,
      CancellationToken ct
    )
    {
      ValidationResult validation
        = await _contractPatchValidator.ValidateAsync(dto, ct);
      if (!validation.IsValid)
        return BadRequest(validation.Errors.Select(er => new
        {
          field = er.PropertyName,
          message = er.ErrorMessage
        }));

      RequestResult<ContractGetDto> result
        = await _service.UpdateStatusAsync(id, dto, ct);

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
      ValidationResult validation
        = await _deliverablePostValidator.ValidateAsync(dto, ct);
      if (!validation.IsValid)
        return BadRequest(validation.Errors.Select(er => new
        {
          field = er.PropertyName,
          message = er.ErrorMessage
        }));

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
