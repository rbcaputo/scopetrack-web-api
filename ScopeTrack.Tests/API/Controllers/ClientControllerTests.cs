using FluentAssertions;
using ScopeTrack.Application.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace ScopeTrack.Tests.API.Controllers
{
  [Collection("ApiTests")]
  public sealed class ClientControllerTests(TestApiFactoryFixture fixture)
  {
    private readonly HttpClient _client = fixture.Factory.CreateClient();
    private static ClientPostDto UniqueClientDto(string name = "Acme")
      => new(name, $"contact_{Guid.NewGuid()}@example.com");
    private async Task<ClientGetDto> CreateClientAsync(ClientPostDto? dto = null)
    {
      dto ??= UniqueClientDto();
      var response = await _client.PostAsJsonAsync("/api/clients", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var client = await response.Content.ReadFromJsonAsync<ClientGetDto>();

      client.Should().NotBeNull();

      return client!;
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_Returns201AndClient()
    {
      var dto = UniqueClientDto();
      var response = await _client.PostAsJsonAsync("/api/clients", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var client = await response.Content.ReadFromJsonAsync<ClientGetDto>();

      client.Should().NotBeNull();
      client!.Name.Should().Be(dto.Name);
      client.Email.Should().Be(dto.Email);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDto_Returns400()
    {
      var dto = new ClientPostDto("", "not-an-email");
      var response = await _client.PostAsJsonAsync("/api/clients", dto);

      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_Returns409()
    {
      var dto = UniqueClientDto();

      await _client.PostAsJsonAsync("/api/clients", dto);

      var response = await _client.PostAsJsonAsync("/api/clients", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_Returns404()
    {
      var dto = new ClientPutDto("New", "new@email.com");
      var response = await _client.PutAsJsonAsync($"/api/clients/{Guid.NewGuid()}", dto);

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WithValidClient_Returns200AndUpdatedClient()
    {
      var client = await CreateClientAsync();
      var updateDto = new ClientPutDto("Updated Name", $"new_{Guid.NewGuid()}@example.com");
      var response = await _client.PutAsJsonAsync($"/api/clients/{client.Id}", updateDto);

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var updated = await response.Content.ReadFromJsonAsync<ClientGetDto>();

      updated.Should().NotBeNull();
      updated!.Name.Should().Be(updateDto.Name);
      updated.Email.Should().Be(updateDto.Email);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenNotFound_Returns404()
    {
      var response = await _client.PostAsync($"/api/clients/{Guid.NewGuid()}/toggle-status", null);

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleStatusAsync_WithValidClient_Returns200AndTogglesStatus()
    {
      var client = await CreateClientAsync();
      var response = await _client.PostAsync($"/api/clients/{client.Id}/toggle-status", null);

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var updated = await response.Content.ReadFromJsonAsync<ClientGetDto>();

      updated.Should().NotBeNull();
      updated!.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task AddContractAsync_WithInvalidDto_Returns400()
    {
      var client = await CreateClientAsync();
      var invalidDto = new ContractPostDto("", "", "");
      var response = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/contracts", invalidDto);

      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContractAsync_WhenClientNotFound_Returns404()
    {
      var dto = new ContractPostDto("Website", "Desc", "FixedPrice");
      var response = await _client.PostAsJsonAsync($"/api/clients/{Guid.NewGuid()}/contracts", dto);

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddContractAsync_WithValidClient_Returns201AndContract()
    {
      var client = await CreateClientAsync();
      var dto = new ContractPostDto("Website Contract", "Desc", "FixedPrice");
      var response = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/contracts", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var contract = await response.Content.ReadFromJsonAsync<ContractGetDto>();

      contract.Should().NotBeNull();
      contract!.Title.Should().Be(dto.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
      var response = await _client.GetAsync($"/api/clients/{Guid.NewGuid()}");

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_Returns200AndClient()
    {
      var client = await CreateClientAsync();
      var response = await _client.GetAsync($"/api/clients/{client.Id}");

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var retrieved = await response.Content.ReadFromJsonAsync<ClientGetDto>();

      retrieved.Should().NotBeNull();
      retrieved!.Name.Should().Be(client.Name);
    }

    [Fact]
    public async Task GetAllAsync_Returns200AndList()
    {
      var c1 = await CreateClientAsync(new ClientPostDto("Abc", $"a_{Guid.NewGuid()}@example.com"));
      var c2 = await CreateClientAsync(new ClientPostDto("Bcd", $"b_{Guid.NewGuid()}@example.com"));

      var response = await _client.GetAsync("/api/clients");

      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var clients = await response.Content.ReadFromJsonAsync<List<ClientGetDto>>();

      clients.Should().NotBeNull();
      clients!.Should().ContainSingle(c => c.Id == c1.Id);
      clients.Should().ContainSingle(c => c.Id == c2.Id);
    }
  }
}
