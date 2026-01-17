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
  public class ClientController(
    IValidator<ClientPostDTO> clientPostValidator,
    IValidator<ClientPutDTO> putValidator,
    IValidator<ContractPostDTO> contractPostValidator,
    IClientService service

  ) : ControllerBase
  {
    private readonly IValidator<ClientPostDTO> _clientPostValidator
      = clientPostValidator;
    private readonly IValidator<ClientPutDTO> _putValidator
      = putValidator;
    private readonly IValidator<ContractPostDTO> _contractPostValidator
      = contractPostValidator;
    private readonly IClientService _service = service;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
      [FromBody] ClientPostDTO dto,
      CancellationToken ct
    )
    {
      ValidationResult validationResult
        = await _clientPostValidator.ValidateAsync(dto, ct);
      if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => new
        {
          field = e.PropertyName,
          message = e.ErrorMessage
        }));

      RequestResult<ClientGetDTO> requestResult
        = await _service.CreateAsync(dto, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : Conflict(requestResult.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(
      Guid id,
      [FromBody] ClientPutDTO dto,
      CancellationToken ct
    )
    {
      ValidationResult validationResult
        = await _putValidator.ValidateAsync(dto, ct);
      if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => new
        {
          field = e.PropertyName,
          message = e.ErrorMessage
        }));

      RequestResult<ClientGetDTO> requestResult
        = await _service.UpdateAsync(id, dto, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> ToggleStatusAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<ClientGetDTO> requestResult
        = await _service.ToggleStatusAsync(id, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }

    [HttpPost("{id}/contracts")]
    public async Task<IActionResult> AddContractAsync(
      Guid id,
      [FromBody] ContractPostDTO dto,
      CancellationToken ct
    )
    {
      ValidationResult validationResult
        = await _contractPostValidator.ValidateAsync(dto, ct);
      if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => new
        {
          field = e.PropertyName,
          message = e.ErrorMessage
        }));

      RequestResult<ContractGetDTO> requestResult
        = await _service.AddContractAsync(id, dto, ct);

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
      RequestResult<ClientGetDTO> requestResult
        = await _service.GetByIDAsync(id, ct);

      return requestResult.IsSuccess
        ? Ok(requestResult.Value)
        : NotFound(requestResult.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
      RequestResult<IReadOnlyList<ClientGetDTO>> requestResult =
        await _service.GetAllAsync(ct);

      return Ok(requestResult.Value);
    }
  }
}
