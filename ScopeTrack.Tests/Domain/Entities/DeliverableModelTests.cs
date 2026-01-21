using FluentAssertions;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Tests.Domain.Entities
{
  public sealed class DeliverableModelTests
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

      deliverable.Id.Should().NotBeEmpty();
      deliverable.ContractId.Should().Be(_contractId);
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
    public void ChangeStatus_FromInProgressToPending_WhenContractActive_ThrowsInvalidOperationException()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );
      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);

      var act = () => deliverable.ChangeStatus(
        DeliverableStatus.Pending,
        ContractStatus.Active
      );

      act.Should().Throw<InvalidOperationException>()
        .WithMessage($"Invalid status transition from InProgress to Pending");
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

    [Fact]
    public void ChangeStatus_FromCancelled_AlwaysThrowsTerminalStateException()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );

      deliverable.ChangeStatus(DeliverableStatus.Cancelled, ContractStatus.Active);

      var act = () => deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot change status of a cancelled deliverable");
    }

    [Fact]
    public void ChangeStatus_FromCompleted_AlwaysThrowsTerminalStateException()
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );

      deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);
      deliverable.ChangeStatus(DeliverableStatus.Completed, ContractStatus.Active);

      var act = () => deliverable.ChangeStatus(DeliverableStatus.Cancelled, ContractStatus.Active);

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot change status of a completed deliverable");
    }

    [Theory]
    [InlineData(DeliverableStatus.Pending)]
    [InlineData(DeliverableStatus.InProgress)]
    public void ChangeStatus_FromSameMutableStatus_ThrowsAlreadyInStatus(
      DeliverableStatus status
    )
    {
      var deliverable = new DeliverableModel(
        _contractId,
        "Homepage Design",
        "Description",
        null
      );

      if (status == DeliverableStatus.InProgress)
        deliverable.ChangeStatus(DeliverableStatus.InProgress, ContractStatus.Active);

      var act = () => deliverable.ChangeStatus(status, ContractStatus.Active);

      act.Should().Throw<InvalidOperationException>()
        .WithMessage($"Deliverable status is already {status}");
    }
  }
}
