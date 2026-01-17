using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;

namespace ScopeTrack.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ClientController(IClientService service) : ControllerBase
  {
    private readonly IClientService _service = service;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
      ClientPostDTO dto,
      CancellationToken ct
    )
    {
      Result<ClientGetDTO> result = await _service.CreateAsync(dto, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : Conflict(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(
      Guid id,
      ClientPutDTO dto,
      CancellationToken ct
    )
    {
      Result<ClientGetDTO> result = await _service.UpdateAsync(id, dto, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> ToggleStatusAsync(
      Guid id,
      CancellationToken ct
    )
    {
      Result<ClientGetDTO> result = await _service.ToggleStatusAsync(id, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> AddContractAsync(
      Guid id,
      ContractPostDTO dto,
      CancellationToken ct
    )
    {
      Result<ContractGetDTO> result = await _service.AddContractAsync(id, dto, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      Result<ClientGetDTO> result = await _service.GetByIDAsync(id, ct);

      return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
      Result<IReadOnlyList<ClientGetDTO>> result =
        await _service.GetAllAsync(ct);

      return Ok(result.Value);
    }
  }
}
