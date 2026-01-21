using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Services;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Tests.Application.Services
{
  public sealed class DeliverableServiceTests
  {
    private static ScopeTrackDbContext CreateDbContext()
    {
      var options = new DbContextOptionsBuilder<ScopeTrackDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      return new ScopeTrackDbContext(options);
    }

    private static async Task<(ContractModel contract, DeliverableModel deliverable)> SeedAsync(
      ScopeTrackDbContext context
    )
    {
      var contract = new ContractModel(
        Guid.NewGuid(),
        "Test Contract",
        "Desc",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        Guid.NewGuid(),
        "Deliverable 1 Title",
        "Desc",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();
      context.Contracts.Add(contract);

      await context.SaveChangesAsync();
      return (contract, deliverable);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenDeliverableNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new DeliverableService(context);
      var dto = new DeliverablePatchDto("Completed");
      var result = await service.UpdateStatusAsync(
          Guid.NewGuid(),
          dto,
          CancellationToken.None
        );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Deliverable not found");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenContractNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new DeliverableService(context);
      var deliverable = new DeliverableModel(
        Guid.NewGuid(),
        "Deliverable 1 Title",
        "Desc",
        null
      );

      context.Deliverables.Add(deliverable);
      await context.SaveChangesAsync();

      var dto = new DeliverablePatchDto("Completed");
      var result = await service.UpdateStatusAsync(
        deliverable.Id,
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Contract not found");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenInvalidStatus_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new DeliverableService(context);
      var (_, deliverable) = await SeedAsync(context);
      var dto = new DeliverablePatchDto("NotARealStatus");
      var result = await service.UpdateStatusAsync(
        deliverable.Id,
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Invalid deliverable status");
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidStatus_UpdatesStatus()
    {
      using var context = CreateDbContext();

      var service = new DeliverableService(context);
      var (contract, deliverable) = await SeedAsync(context);

      deliverable.ChangeStatus(
        DeliverableStatus.InProgress,
        ContractStatus.Active
      );
      await context.SaveChangesAsync();

      var dto = new DeliverablePatchDto(DeliverableStatus.Completed.ToString());
      var result = await service.UpdateStatusAsync(
        deliverable.Id,
        dto,
        CancellationToken.None
      );

      result.IsSuccess.Should().BeTrue();
      result.Value!.Status.Should().Be(DeliverableStatus.Completed.ToString());

      var reloaded
        = await context.Deliverables.SingleAsync(d => d.Id == deliverable.Id);

      reloaded.Status.Should().Be(DeliverableStatus.Completed);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
      using var context = CreateDbContext();

      var service = new DeliverableService(context);
      var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

      result.IsSuccess.Should().BeFalse();
      result.Error.Should().Be("Deliverable not found");
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsDeliverable()
    {
      using var context = CreateDbContext();

      var service = new DeliverableService(context);
      var (_, deliverable) = await SeedAsync(context);
      var result = await service.GetByIdAsync(deliverable.Id, CancellationToken.None);

      result.IsSuccess.Should().BeTrue();
      result.Value!.Id.Should().Be(deliverable.Id);
      result.Value.Title.Should().Be(deliverable.Title);
      result.Value.Status.Should().Be(deliverable.Status.ToString());
    }
  }
}
