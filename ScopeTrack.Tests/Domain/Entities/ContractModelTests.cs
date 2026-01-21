using FluentAssertions;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Tests.Domain.Entities
{
  public sealed class ContractModelTests
  {
    private readonly Guid _clientId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidInputs_CreatesContract()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Complete redesign of marketing site",
        ContractType.FixedPrice
      );

      contract.Id.Should().NotBeEmpty();
      contract.ClientId.Should().Be(_clientId);
      contract.Title.Should().Be("Website Redesign");
      contract.Description.Should().Be("Complete redesign of marketing site");
      contract.Type.Should().Be(ContractType.FixedPrice);
      contract.Status.Should().Be(ContractStatus.Draft);
      contract.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
      contract.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
      contract.Deliverables.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullDescription_SetsEmptyString()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        null,
        ContractType.TimeBased
      );

      contract.Description.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidTitle_ThrowsArgumentException(string title)
    {
      var act = () => new ContractModel(
        _clientId,
        title,
        "Description",
        ContractType.FixedPrice
      );

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyClientId_ThrowsArgumentException()
    {
      var act = () => new ContractModel(
        Guid.Empty,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activate_FromDraftWithDeliverables_ActivatesSuccessfully()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();

      contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void Activate_FromDraftWithoutDeliverables_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var act = () => contract.Activate();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot activate contract without at least one deliverable");
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();

      var act = () => contract.Activate();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Contract is already active");
    }

    [Fact]
    public void Activate_FromCompleted_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();
      contract.Complete();

      var act = () => contract.Activate();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot activate a completed contract");
    }

    [Fact]
    public void Activate_FromArchived_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      contract.Archive();

      var act = () => contract.Activate();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot activate an archived contract");
    }

    [Fact]
    public void Complete_FromActive_CompletesSuccessfully()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();
      contract.Complete();

      contract.Status.Should().Be(ContractStatus.Completed);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();
      contract.Complete();

      var act = () => contract.Complete();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Contract is already completed");
    }

    [Fact]
    public void Complete_FromArchived_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      contract.Archive();

      var act = () => contract.Complete();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot complete an archived contract");
    }

    [Fact]
    public void Archive_FromDraft_ArchivesSuccessfully()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      contract.Archive();

      contract.Status.Should().Be(ContractStatus.Archived);
    }

    [Fact]
    public void Archive_FromActive_ArchivesSuccessfully()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);
      contract.Activate();
      contract.Archive();

      contract.Status.Should().Be(ContractStatus.Archived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      contract.Archive();

      var act = () => contract.Archive();

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Contract is already archived");
    }

    [Fact]
    public void AddDeliverable_ToNonArchivedContract_AddsSuccessfully()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);

      contract.Deliverables.Should().ContainSingle();
      contract.Deliverables[0].Should().Be(deliverable);
    }

    [Fact]
    public void AddDeliverable_ToArchivedContract_ThrowsInvalidOperationException()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      contract.Archive();

      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      var act = () => contract.AddDeliverable(deliverable);

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot add deliverables to an archived contract");
    }

    [Fact]
    public void AddDeliverable_UpdatesTimestamp()
    {
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );
      var originalUpdatedAt = contract.UpdatedAt;

      Thread.Sleep(10);

      var deliverable = new DeliverableModel(
        contract.Id,
        "Homepage Design",
        "Design homepage",
        null
      );

      contract.AddDeliverable(deliverable);

      contract.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(nameof(ContractModel.Activate))]
    [InlineData(nameof(ContractModel.Complete))]
    [InlineData(nameof(ContractModel.Archive))]
    public void StatusChangeMethods_UpdateTimestamp(string methodName)
    {
      // Arrange
      var contract = new ContractModel(
        _clientId,
        "Website Redesign",
        "Description",
        ContractType.FixedPrice
      );

      if (methodName == nameof(ContractModel.Activate))
      {
        var deliverable = new DeliverableModel(
          contract.Id,
          "Homepage Design",
          "Design Homepage",
          null
        );

        contract.AddDeliverable(deliverable);
      }

      if (methodName == nameof(ContractModel.Complete))
      {
        var deliverable = new DeliverableModel(
          contract.Id,
          "Homepage Design",
          "Design homepage",
          null
        );

        contract.AddDeliverable(deliverable);
        contract.Activate();
      }

      var originalUpdatedAt = contract.UpdatedAt;

      Thread.Sleep(10);

      switch (methodName)
      {
        case nameof(ContractModel.Activate):
          contract.Activate();
          break;
        case nameof(ContractModel.Complete):
          contract.Complete();
          break;
        case nameof(ContractModel.Archive):
          contract.Archive();
          break;
      }

      contract.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
  }
}
