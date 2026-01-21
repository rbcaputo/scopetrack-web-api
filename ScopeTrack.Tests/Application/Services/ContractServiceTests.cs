using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Services;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Tests.Application.Services
{
  public sealed class ContractServiceTests
  {
    private static ScopeTrackDbContext CreateDbContext()
    {
      var options = new DbContextOptionsBuilder<ScopeTrackDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      return new ScopeTrackDbContext(options);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenContractNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var dto = new ContractPatchDto("Active");
      var result = await service.UpdateStatusAsync(
        Guid.NewGuid(),
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Contract not found");
    }

    [Fact]
    public async Task UpdateStatusAsync_WithInvalidStatus_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var contract = new ContractModel(
        Guid.NewGuid(),
        "Website Contract",
        "Desc",
        ContractType.FixedPrice
      );

      context.Contracts.Add(contract);
      await context.SaveChangesAsync();

      var dto = new ContractPatchDto("IvalidStatus");
      var result = await service.UpdateStatusAsync(
        contract.Id,
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Invalid contract status");
    }

    [Fact]
    public async Task UpdateStatusAsync_ToActive_UpdatesStatusAndReturnsSuccess()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var contract = new ContractModel(
        Guid.NewGuid(),
        "Website Contract",
        "Desc",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Deliverable",
        "Desc",
        null
      );

      contract.AddDeliverable(deliverable);
      context.Contracts.Add(contract);
      context.Deliverables.Add(deliverable);
      await context.SaveChangesAsync();

      var dto = new ContractPatchDto(ContractStatus.Active.ToString());
      var result = await service.UpdateStatusAsync(contract.Id, dto, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Status.Should().Be(ContractStatus.Active.ToString());

      var reloaded = await context.Contracts.SingleAsync(c => c.Id == contract.Id);

      reloaded.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public async Task AddDeliverableAsync_WhenContractNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var dto = new DeliverablePostDto("Homepage Deliverable", "Desc", null);
      var result = await service.AddDeliverableAsync(
        Guid.NewGuid(),
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Contract not found");
    }

    [Fact]
    public async Task AddDeliverableAsync_WithValidContract_AddsDeliverable()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var contract = new ContractModel(
        Guid.NewGuid(),
        "Website Contract",
        "Desc",
        ContractType.FixedPrice
      );

      context.Contracts.Add(contract);
      await context.SaveChangesAsync();

      var dto = new DeliverablePostDto("Homepage Deliverable", "Desc", null);
      var result = await service.AddDeliverableAsync(
        contract.Id,
        dto, CancellationToken.None
      );

      result.IsSuccess.Should().BeTrue();
      result.Value!.Title.Should().Be("Homepage Deliverable");

      var reloaded = await context.Contracts
        .Include(c => c.Deliverables)
        .SingleAsync(c => c.Id == contract.Id);

      reloaded.Deliverables.Should().HaveCount(1);
      reloaded.Deliverables[0].Title.Should().Be("Homepage Deliverable");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Contract not found");
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsContractWithDeliverables()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var contract = new ContractModel(
        Guid.NewGuid(),
        "Website Contract",
        "Desc",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Deliverable",
        "Desc",
        null
      );

      contract.AddDeliverable(deliverable);
      context.Contracts.Add(contract);
      context.Deliverables.Add(deliverable);
      await context.SaveChangesAsync();

      var result = await service.GetByIdAsync(contract.Id, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Deliverables.Should().HaveCount(1);
      result.Value.Deliverables[0].Title.Should().Be("Homepage Deliverable");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllContractsWithDeliverables()
    {
      using var context = CreateDbContext();

      var service = new ContractService(context);
      var contract1 = new ContractModel(
        Guid.NewGuid(),
        "Website Contract",
        "Desc",
        ContractType.FixedPrice
      );
      var contract2 = new ContractModel(
        Guid.NewGuid(),
        "Mobile App Contract",
        "Desc",
        ContractType.TimeBased
      );

      context.Contracts.AddRange(contract1, contract2);
      await context.SaveChangesAsync();

      var result = await service.GetAllAsync(CancellationToken.None);

      result.Should().HaveCount(2);
      result.Select(c => c.Title)
        .Should().Contain(["Website Contract", "Mobile App Contract"]);
    }
  }
}
