using Microsoft.AspNetCore.Mvc;
using ScopeTrack.Application;
using ScopeTrack.Application.Dtos;
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
      [FromBody] ClientPostDto dto,
      CancellationToken ct
    )
    {
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
      RequestResult<ClientGetDto> result
        = await _service.UpdateAsync(id, dto, ct);

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
