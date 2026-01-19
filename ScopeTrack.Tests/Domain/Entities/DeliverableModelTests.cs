using FluentAssertions;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Tests.Domain.Entities
{
  public class DeliverableModelTests
  {
    private readonly Guid _contractId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidInputs_CreatesDeliverable()
    {
      var dueDate = DateTime.UtcNow.AddDays(30);
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Design new homepage layout",
        dueDate
      );

      deliverable.ID.Should().NotBeEmpty();
      deliverable.ContractID.Should().Be(_contractId);
      deliverable.Title.Should().Be("Homepage Design");
      deliverable.Description.Should().Be("Design new homepage layout");
      deliverable.Status.Should().Be(DeliverableStatus.Pending);
      deliverable.DueDate.Should().Be(dueDate);
      deliverable.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
      deliverable.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithNullDescription_SetsEmptyString()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        null,
        null
      );

      deliverable.Description.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullDueDate_SetsNull()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );

      deliverable.DueDate.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidTitle_ThrowsArgumentException(string title)
    {
      var act = () => new DeliverableModel(
        _contractId,
        title,
        "Description",
        null
      );

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyContractId_ThrowsArgumentException()
    {
      var act = () => new DeliverableModel(
        Guid.Empty,
        "Homepage Design",
        "Description",
        null
      );

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeStatus_FromPendingToInProgress_WhenContractActive_ChangesSuccessfully()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );
      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);

      deliverable.Status.Should().Be(DeliverableStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_FromInProgressToCompleted_WhenContractActive_ChangesSuccessfully()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );
      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);
      deliverable.ChangeStatus(DeliverableStatus.Completed, ContractStatus.Active);

      deliverable.Status.Should().Be(DeliverableStatus.Completed);
    }

    [Fact]
    public void ChangeStatus_FromInProgressToPending_WhenContractActive_ChangesSuccessfully()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );
      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);
      deliverable.ChangeStatus(DeliverableStatus.Pending, ContractStatus.Active);

      deliverable.Status.Should().Be(DeliverableStatus.Pending);
    }

    [Fact]
    public void ChangeStatus_WhenContractNotActive_ThrowsInvalidOperationException()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );

      var act = () => deliverable.ChangeStatus(
        DeliverableStatus.InProgress,
        ContractStatus.Draft
      );

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot change deliverable status when contract is not active");
    }

    [Theory]
    [InlineData(DeliverableStatus.Pending, DeliverableStatus.Completed)]
    [InlineData(DeliverableStatus.Pending, DeliverableStatus.Cancelled)]
    [InlineData(DeliverableStatus.Completed, DeliverableStatus.Pending)]
    [InlineData(DeliverableStatus.Completed, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Completed, DeliverableStatus.Cancelled)]
    [InlineData(DeliverableStatus.Cancelled, DeliverableStatus.Pending)]
    [InlineData(DeliverableStatus.Cancelled, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Cancelled, DeliverableStatus.Completed)]
    public void ChangeStatus_WithInvalidTransition_ThrowsInvalidOperationException(
      DeliverableStatus currentStatus,
      DeliverableStatus newStatus
    )
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );


      if (currentStatus == DeliverableStatus.InProgress)
        deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);
      else if (currentStatus == DeliverableStatus.Completed)
      {
        deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);
        deliverable.ChangeStatus(DeliverableStatus.Completed, ContractStatus.Active);
      }

      var act = () => deliverable.ChangeStatus(newStatus, ContractStatus.Active);

      // Assert
      act.Should().Throw<InvalidOperationException>()
        .WithMessage($"Invalid status transition from {currentStatus} to {newStatus}");
    }

    [Fact]
    public void ChangeStatus_UpdatesTimestamp()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );
      var originalUpdatedAt = deliverable.UpdatedAt;
      Thread.Sleep(10);
      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);

      deliverable.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void ChangeStatus_FromInProgressToInProgress_ThrowsInvalidOperationException()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );
      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);

      var act = () => deliverable.ChangeStatus(
        DeliverableStatus.InProgress,
        ContractStatus.Active
      );

      act.Should().Throw<InvalidOperationException>();
    }
  }
}
