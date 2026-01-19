using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Services;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Tests.Application.Services
{
  public sealed class ClientServiceTests
  {
    private readonly ScopeTrackDbContext _context;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
      var options = new DbContextOptionsBuilder<ScopeTrackDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

      _context = new ScopeTrackDbContext(options);
      _service = new ClientService(_context);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccessResult()
    {
      var dto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var result = await _service.CreateAsync(dto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().NotBeNull();
      result.Value!.Name.Should().Be("Acme Corp");
      result.Value.Email.Should().Be("contact@acme.com");
      result.Value.Status.Should().Be(ClientStatus.Active.ToString());
      result.Value.Contracts.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ReturnsFailureResult()
    {
      var dto1 = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var dto2 = new ClientPostDTO("Different Name", "contact@acme.com");
      await _service.CreateAsync(dto1, CancellationToken.None);

      var result = await _service.CreateAsync(dto2, CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client already exists");
    }

    [Fact]
    public async Task CreateAsync_PersistsToDatabase()
    {
      var dto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      await _service.CreateAsync(dto, CancellationToken.None);

      var clientInDb = await _context.Clients.FirstOrDefaultAsync();

      clientInDb.Should().NotBeNull();
      clientInDb!.Name.Should().Be("Acme Corp");
      clientInDb.Email.Should().Be("contact@acme.com");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsSuccessResult()
    {
      var createDto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var createResult = await _service.CreateAsync(createDto, CancellationToken.None);
      var clientId = createResult.Value!.ID;
      var updateDto = new ClientPutDTO("Updated Corp", "updated@acme.com");
      var result = await _service.UpdateAsync(clientId, updateDto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().NotBeNull();
      result.Value!.Name.Should().Be("Updated Corp");
      result.Value.Email.Should().Be("updated@acme.com");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsFailureResult()
    {
      var nonExistentId = Guid.NewGuid();
      var updateDto = new ClientPutDTO("Updated Corp", "updated@acme.com");
      var result = await _service.UpdateAsync(nonExistentId, updateDto, CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task ToggleStatusAsync_FromActiveToInactive_ReturnsSuccessResult()
    {
      var createDto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var createResult = await _service.CreateAsync(createDto, CancellationToken.None);
      var clientId = createResult.Value!.ID;
      var result = await _service.ToggleStatusAsync(clientId, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Status.Should().Be(ClientStatus.Inactive.ToString());
    }

    [Fact]
    public async Task ToggleStatusAsync_FromInactiveToActive_ReturnsSuccessResult()
    {
      var createDto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var createResult = await _service.CreateAsync(createDto, CancellationToken.None);
      var clientId = createResult.Value!.ID;
      await _service.ToggleStatusAsync(clientId, CancellationToken.None);

      var result = await _service.ToggleStatusAsync(clientId, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Status.Should().Be(ClientStatus.Active.ToString());
    }

    [Fact]
    public async Task ToggleStatusAsync_WithNonExistentId_ReturnsFailureResult()
    {
      var nonExistentId = Guid.NewGuid();
      var result = await _service.ToggleStatusAsync(nonExistentId, CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task AddContractAsync_ToActiveClient_ReturnsSuccessResult()
    {
      var clientDto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var clientResult = await _service.CreateAsync(clientDto, CancellationToken.None);
      var clientId = clientResult.Value!.ID;
      var contractDto = new ContractPostDTO(
        clientId,
        "Website Redesign",
        "Complete redesign",
        "FixedPrice"
      );
      var result = await _service.AddContractAsync(clientId, contractDto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().NotBeNull();
      result.Value!.Title.Should().Be("Website Redesign");
      result.Value.Status.Should().Be(ContractStatus.Draft.ToString());
    }

    [Fact]
    public async Task AddContractAsync_WithNonExistentClient_ReturnsFailureResult()
    {
      var nonExistentId = Guid.NewGuid();
      var contractDto = new ContractPostDTO(
        nonExistentId,
        "Website Redesign",
        "Complete redesign",
        "FixedPrice"
      );
      var result = await _service.AddContractAsync(
        nonExistentId,
        contractDto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task GetByIDAsync_WithExistingId_ReturnsSuccessResult()
    {
      var createDto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var createResult = await _service.CreateAsync(createDto, CancellationToken.None);
      var clientId = createResult.Value!.ID;
      var result = await _service.GetByIDAsync(clientId, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().NotBeNull();
      result.Value!.ID.Should().Be(clientId);
      result.Value.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetByIDAsync_WithNonExistentId_ReturnsFailureResult()
    {
      var nonExistentId = Guid.NewGuid();
      var result = await _service.GetByIDAsync(nonExistentId, CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleClients_ReturnsAllClients()
    {
      await _service.CreateAsync(
        new ClientPostDTO("Client 1", "client1@test.com"),
        CancellationToken.None
      );
      await _service.CreateAsync(
        new ClientPostDTO("Client 2", "client2@test.com"),
        CancellationToken.None
      );

      var result = await _service.GetAllAsync(CancellationToken.None);

      result.Should().HaveCount(2);
      result.Should().Contain(c => c.Name == "Client 1");
      result.Should().Contain(c => c.Name == "Client 2");
    }

    [Fact]
    public async Task GetAllAsync_WithNoClients_ReturnsEmptyList()
    {
      var result = await _service.GetAllAsync(CancellationToken.None);

      result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIDAsync_IncludesContracts()
    {
      var clientDto = new ClientPostDTO("Acme Corp", "contact@acme.com");
      var clientResult = await _service.CreateAsync(clientDto, CancellationToken.None);
      var clientId = clientResult.Value!.ID;
      var contractDto = new ContractPostDTO(
        clientId,
        "Website Redesign",
        "Description",
        "FixedPrice"
      );
      await _service.AddContractAsync(clientId, contractDto, CancellationToken.None);

      var result = await _service.GetByIDAsync(clientId, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Contracts.Should().ContainSingle();
      result.Value.Contracts[0].Title.Should().Be("Website Redesign");
    }
  }
}
