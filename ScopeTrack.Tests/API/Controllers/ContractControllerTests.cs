using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ScopeTrack.Tests.API.Controllers
{
  [Collection("ApiTests")]
  public sealed class ContractControllerTests(TestApiFactoryFixture fixture)
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
    private async Task<DeliverableGetDto> AddDeliverableAsync(ContractGetDto contract, DeliverablePostDto? dto = null)
    {
      dto ??= new DeliverablePostDto("Spec Deliverable", "Spec desc", DateTime.UtcNow.AddDays(7));
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
      var dto = new ContractPatchDto("InvalidStatus");
      var response = await _client.PatchAsJsonAsync($"/api/contract/{contract.Id}", dto);

      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

      var body = await response.Content.ReadFromJsonAsync<JsonElement>();
      var errors = body.GetProperty("errors")
        .EnumerateArray()
        .Select(er => er.GetString())
        .ToList();

      errors.Should().Contain("Invalid contract status");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenNotFound_Returns404()
    {
      var dto = new ContractPatchDto("Active");
      var response = await _client.PatchAsJsonAsync($"/api/contract/{Guid.NewGuid()}", dto);

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidContract_Returns200AndUpdatedContract()
    {
      var clientDto = new ClientPostDto("Client Name", "client@email.com");
      var clientResponse = await _client.PostAsJsonAsync("/api/client", clientDto);

      clientResponse.EnsureSuccessStatusCode();

      var client = await clientResponse.Content.ReadFromJsonAsync<ClientGetDto>();

      var contractDto = new ContractPostDto("Contract Title", "Desc", "FixedPrice");
      var contractResponse = await _client.PostAsJsonAsync(
        $"/api/client/{client!.Id}/contracts",
        contractDto
      );

      contractResponse.EnsureSuccessStatusCode();

      var contract = await contractResponse.Content.ReadFromJsonAsync<ContractGetDto>();
      var deliverableDto = new DeliverablePostDto("Deliverable Title", "Desc", null);
      var deliverableResponse = await _client.PostAsJsonAsync(
        $"/api/contract/{contract!.Id}/deliverables",
        deliverableDto
      );

      deliverableResponse.EnsureSuccessStatusCode();

      var patchDto = new ContractPatchDto("Active");
      var response = await _client.PatchAsJsonAsync(
        $"/api/contract/{contract.Id}",
        patchDto
      );

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var updated = await response.Content.ReadFromJsonAsync<ContractGetDto>();

      updated.Should().NotBeNull();
      updated.Status.Should().Be("Active");
    }

    [Fact]
    public async Task AddDeliverableAsync_WithInvalidDto_Returns400()
    {
      var response = await _client.PostAsJsonAsync(
        $"/api/contract/{Guid.NewGuid()}/deliverables",
        new DeliverablePostDto("", "", DateTime.MinValue)
      );

      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDeliverableAsync_WhenContractNotFound_Returns404()
    {
      var dto = new DeliverablePostDto("Spec Deliverable", "Desc", DateTime.UtcNow.AddDays(7));
      var response = await _client.PostAsJsonAsync($"/api/contract/{Guid.NewGuid()}/deliverables", dto);

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddDeliverableAsync_WithValidContract_Returns201AndDeliverable()
    {
      var client = await CreateClientAsync();
      var contract = await CreateContractAsync(client);

      var deliverable = await AddDeliverableAsync(contract);

      deliverable.Title.Should().Be("Spec Deliverable");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
      var response = await _client.GetAsync($"/api/contract/{Guid.NewGuid()}");

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_Returns200AndContract()
    {
      var client = await CreateClientAsync();
      var contract = await CreateContractAsync(client);
      var response = await _client.GetAsync($"/api/contract/{contract.Id}");

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var retrieved = await response.Content.ReadFromJsonAsync<ContractGetDto>();

      retrieved.Should().NotBeNull();
      retrieved!.Title.Should().Be(contract.Title);
    }

    [Fact]
    public async Task GetAllAsync_Returns200AndList()
    {
      var client = await CreateClientAsync();

      await CreateContractAsync(client, new ContractPostDto("Website A", "Desc A", "FixedPrice"));
      await CreateContractAsync(client, new ContractPostDto("Website B", "Desc B", "TimeAndMaterial"));

      var response = await _client.GetAsync("/api/contract");

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var contracts = await response.Content.ReadFromJsonAsync<List<ContractGetDto>>();

      contracts.Should().NotBeNull();
      contracts!.Should().HaveCountGreaterThanOrEqualTo(2);
    }
  }
}
