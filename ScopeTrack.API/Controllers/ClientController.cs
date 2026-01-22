using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [ApiController]
  [Route("api/clients")]
  public class ClientController(
    IClientService service,
    IValidator<ClientPostDto> clientPostValidator,
    IValidator<ClientPutDto> clientPutValidator,
    IValidator<ContractPostDto> contractPostValidator
  ) : ControllerBase
  {
    private readonly IClientService _service = service;
    private readonly IValidator<ClientPostDto> _clientPostValidator = clientPostValidator;
    private readonly IValidator<ClientPutDto> _clientPutValidator = clientPutValidator;
    private readonly IValidator<ContractPostDto> _contractPostValidator = contractPostValidator;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
      [FromBody] ClientPostDto dto,
      CancellationToken ct
    )
    {
      ValidationResult validation
        = await _clientPostValidator.ValidateAsync(dto, ct);
      if (!validation.IsValid)
        return BadRequest(new
        {
          errors = validation.Errors.Select(er => er.ErrorMessage)
        });

      RequestResult<ClientGetDto> result
        = await _service.CreateAsync(dto, ct);

      return result.IsSuccess
        ? CreatedAtRoute(
            "GetClientById",
            new { id = result.Value!.Id },
            result.Value
          )
        : Conflict(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(
      Guid id,
      [FromBody] ClientPutDto dto,
      CancellationToken ct
    )
    {
      ValidationResult validation
        = await _clientPutValidator.ValidateAsync(dto, ct);
      if (!validation.IsValid)
        return BadRequest(new
        {
          errors = validation.Errors.Select(er => er.ErrorMessage)
        });

      RequestResult<ClientGetDto> result
        = await _service.UpdateAsync(id, dto, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatusAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<ClientGetDto> result
        = await _service.ToggleStatusAsync(id, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpPost("{id}/contracts")]
    public async Task<IActionResult> AddContractAsync(
      Guid id,
      [FromBody] ContractPostDto dto,
      CancellationToken ct
    )
    {
      RequestResult<ClientGetDto> client = await _service.GetByIdAsync(id, ct);
      if (!client.IsSuccess)
        return NotFound(new { error = client.Error });

      ValidationResult validation
        = await _contractPostValidator.ValidateAsync(dto, ct);
      if (!validation.IsValid)
        return BadRequest(new
        {
          errors = validation.Errors.Select(er => er.ErrorMessage)
        });

      RequestResult<ContractGetDto> result
        = await _service.AddContractAsync(id, dto, ct);

      return result.IsSuccess
        ? CreatedAtRoute(
            "GetContractById",
            new { id = result.Value!.Id },
            result.Value
        )
        : NotFound(result.Error);
    }

    [HttpGet("{id}", Name = "GetClientById")]
    public async Task<IActionResult> GetByIdAsync(
      Guid id,
      CancellationToken ct
    )
    {
      RequestResult<ClientGetDto> result
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
