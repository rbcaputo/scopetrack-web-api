using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ScopeTrack.Tests.API.Controllers
{
  [Collection("ApiTests")]
  public sealed class DeliverableControllerTests(TestApiFactoryFixture fixture)
  {
    private readonly HttpClient _client = fixture.Factory.CreateClient();
    private static ClientPostDto UniqueClientDto(string name = "Acme")
      => new(name, $"contact_{Guid.NewGuid()}@example.com");
    private async Task<ClientGetDto> CreateClientAsync(ClientPostDto? dto = null)
    {
      dto ??= UniqueClientDto();
      var response = await _client.PostAsJsonAsync("/api/client", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var client = await response.Content.ReadFromJsonAsync<ClientGetDto>();

      client.Should().NotBeNull();

      return client!;
    }
    private async Task<ContractGetDto> CreateContractAsync(ClientGetDto client, ContractPostDto? dto = null)
    {
      dto ??= new ContractPostDto("Website Contract", "Desc", "FixedPrice");
      var response = await _client.PostAsJsonAsync($"/api/client/{client.Id}/contracts", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var contract = await response.Content.ReadFromJsonAsync<ContractGetDto>();

      contract.Should().NotBeNull();

      return contract!;
    }

    private async Task<DeliverableGetDto> CreateDeliverableAsync(ContractGetDto contract, DeliverablePostDto? dto = null)
    {
      dto ??= new DeliverablePostDto("Spec Deliverable", "Specification", DateTime.UtcNow.AddDays(7));
      var response = await _client.PostAsJsonAsync($"/api/contract/{contract.Id}/deliverables", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var deliverable = await response.Content.ReadFromJsonAsync<DeliverableGetDto>();

      deliverable.Should().NotBeNull();

      return deliverable!;
    }

    [Fact]
    public async Task UpdateStatusAsync_WithInvalidDto_Returns400()
    {
      var client = await CreateClientAsync();
      var contract = await CreateContractAsync(client);
      var deliverable = await CreateDeliverableAsync(contract);
      var dto = new DeliverablePatchDto("InvalidStatus");
      var response = await _client.PatchAsJsonAsync($"/api/deliverable/{deliverable.Id}", dto);

      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

      var body = await response.Content.ReadFromJsonAsync<JsonElement>();
      var errors = body.GetProperty("errors")
        .EnumerateArray()
        .Select(er => er.GetString())
        .ToList();

      errors.Should().Contain("Invalid deliverable status");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenNotFound_Returns404()
    {
      var dto = new DeliverablePatchDto("InProgress");
      var response = await _client.PatchAsJsonAsync($"/api/deliverable/{Guid.NewGuid()}", dto);

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidDeliverable_Returns200AndUpdatedDeliverable()
    {
      var client = await CreateClientAsync();
      var contract = await CreateContractAsync(client);
      var deliverable = await CreateDeliverableAsync(contract);

      using (var scope = fixture.Factory.Services.CreateScope())
      {
        var context = scope.ServiceProvider.GetRequiredService<ScopeTrackDbContext>();
        var contractDb = await context.Contracts
          .Include(c => c.Deliverables)
          .FirstAsync(c => c.Id == contract.Id);

        contractDb!.Activate();

        await context.SaveChangesAsync();
      }

      var patchDto = new DeliverablePatchDto("InProgress");
      var response = await _client.PatchAsJsonAsync(
        $"/api/deliverable/{deliverable.Id}",
        patchDto
      );

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var updated = await response.Content.ReadFromJsonAsync<DeliverableGetDto>();

      updated.Should().NotBeNull();
      updated!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
      var response = await _client.GetAsync($"/api/deliverable/{Guid.NewGuid()}");

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_Returns200AndDeliverable()
    {
      var client = await CreateClientAsync();
      var contract = await CreateContractAsync(client);
      var deliverable = await CreateDeliverableAsync(contract);
      var response = await _client.GetAsync($"/api/deliverable/{deliverable.Id}");

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var retrieved = await response.Content.ReadFromJsonAsync<DeliverableGetDto>();

      retrieved.Should().NotBeNull();
      retrieved!.Title.Should().Be(deliverable.Title);
    }
  }
}