using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Services;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Tests.Application.Services
{
  public sealed class ClientServiceTests
  {
    private static ScopeTrackDbContext CreateDbContext()
    {
      var options = new DbContextOptionsBuilder<ScopeTrackDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      return new ScopeTrackDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_WithNewEmail_ReturnsSuccessAndPersistsClient()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var dto = new ClientPostDto("Acme Corp", "contact@acme.com");
      var result = await service.CreateAsync(dto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Name.Should().Be("Acme Corp");
      result.Value.Email.Should().Be("contact@acme.com");

      var persisted = await context.Clients.SingleAsync();

      persisted.Email.Should().Be("contact@acme.com");
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var existing = new ClientModel("Acme Corp", "contact@acme.com");

      context.Clients.Add(existing);
      await context.SaveChangesAsync();

      var dto = new ClientPostDto("Other", "contact@acme.com");
      var result = await service.CreateAsync(dto, CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client already exists");
    }

    [Fact]
    public async Task UpdateAsync_WhenClientNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var dto = new ClientPutDto("New Name", "new@email.com");
      var result = await service.UpdateAsync(Guid.NewGuid(), dto, CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task UpdateAsync_WithValidClient_UpdatesDetails()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var client = new ClientModel("Acme", "old@email.com");

      context.Clients.Add(client);
      await context.SaveChangesAsync();

      var dto = new ClientPutDto("Acme Updated", "new@email.com");
      var result = await service.UpdateAsync(client.Id, dto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Name.Should().Be("Acme Updated");
      result.Value.Email.Should().Be("new@email.com");

      var reloaded = await context.Clients.SingleAsync(c => c.Id == client.Id);

      reloaded.Name.Should().Be("Acme Updated");
      reloaded.Email.Should().Be("new@email.com");
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenClientNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var result = await service.ToggleStatusAsync(Guid.NewGuid(), CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task ToggleStatusAsync_FromActiveToInactive_UpdatesStatus()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var client = new ClientModel("Acme", "contact@acme.com");

      context.Clients.Add(client);
      await context.SaveChangesAsync();

      var result = await service.ToggleStatusAsync(client.Id, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Status.Should().Be(ClientStatus.Inactive.ToString());

      var reloaded = await context.Clients.SingleAsync(c => c.Id == client.Id);

      reloaded.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public async Task AddContractAsync_WhenClientNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var dto = new ContractPostDto("Website", "Desc", "FixedPrice");
      var result = await service.AddContractAsync(
        Guid.NewGuid(),
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task AddContractAsync_WithValidClient_AddsContract()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var client = new ClientModel("Acme", "contact@acme.com");

      context.Clients.Add(client);
      await context.SaveChangesAsync();

      var dto = new ContractPostDto("Website", "Desc", "FixedPrice");
      var result = await service.AddContractAsync(client.Id, dto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Title.Should().Be("Website");

      var reloaded = await context.Clients
        .Include(c => c.Contracts)
        .SingleAsync(c => c.Id == client.Id);

      reloaded.Contracts.Should().HaveCount(1);
      reloaded.Contracts[0].Title.Should().Be("Website");
    }

    [Fact]
    public async Task AddContractAsync_WhenClientInactive_ThrowsInvalidOperationException()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var client = new ClientModel("Acme", "contact@acme.com");

      context.Clients.Add(client);
      await context.SaveChangesAsync();
      client.ToggleStatus();
      await context.SaveChangesAsync();

      var dto = new ContractPostDto("Website", "Desc", "FixedPrice");
      var act = async () => await service.AddContractAsync(client.Id, dto, CancellationToken.None);

      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Cannot add contracts to an inactive client");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Client not found");
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsClientWithContracts()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var client = new ClientModel("Acme", "contact@acme.com");
      var contract = new ContractModel(
        client.Id,
        "Website",
        "Desc",
        ContractType.FixedPrice
      );

      client.AddContract(contract);
      context.Clients.Add(client);
      await context.SaveChangesAsync();

      var result = await service.GetByIdAsync(client.Id, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Contracts.Should().HaveCount(1);
      result.Value.Contracts[0].Title.Should().Be("Website");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClientsOrderedByStatus()
    {
      using var context = CreateDbContext();

      var service = new ClientService(context);
      var active = new ClientModel("Active", "a@email.com");
      var inactive = new ClientModel("Inactive", "i@email.com");

      inactive.ToggleStatus();
      context.Clients.AddRange(active, inactive);
      await context.SaveChangesAsync();

      var result = await service.GetAllAsync(CancellationToken.None);

      result.Should().HaveCount(2);

      result[0].Status.Should().Be(ClientStatus.Active.ToString());
      result[1].Status.Should().Be(ClientStatus.Inactive.ToString());
    }
  }
}
