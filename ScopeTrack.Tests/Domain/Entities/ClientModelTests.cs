using FluentAssertions;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Tests.Domain.Entities
{
  public sealed class ClientModelTests
  {
    [Fact]
    public void Constructor_WithValidInputs_CreatesClient()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");

      client.Id.Should().NotBeEmpty();
      client.Name.Should().Be("Acme Corp");
      client.Email.Should().Be("contact@acme.com");
      client.Status.Should().Be(ClientStatus.Active);
      client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
      client.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
      client.Contracts.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "contact@acme.com")]
    [InlineData(" ", "contact@acme.com")]
    [InlineData("Acme Corp", "")]
    [InlineData("Acme Corp", " ")]
    public void Constructor_WithInvalidInputs_ThrowsArgumentException(
      string name,
      string email
    )
    {
      var act = () => new ClientModel(name, email);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidInputs_UpdatesClientAndTimestamp()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      var originalUpdatedAt = client.UpdatedAt;

      Thread.Sleep(10);
      client.UpdateDetails("Acme Corporation", "info@acme.com");

      client.Name.Should().Be("Acme Corporation");
      client.Email.Should().Be("info@acme.com");
      client.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData("", "contact@acme.com")]
    [InlineData(" ", "contact@acme.com")]
    [InlineData("Acme Corp", "")]
    [InlineData("Acme Corp", " ")]
    public void UpdateDetails_WithInvalidInputs_ThrowsArgumentException(
    string name,
    string email
  )
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      var act = () => client.UpdateDetails(name, email);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToggleStatus_FromActiveToInactive_ChangesStatus()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      client.Status.Should().Be(ClientStatus.Active);

      client.ToggleStatus();
      client.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public void ToggleStatus_FromInactiveToActive_ChangesStatus()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      client.ToggleStatus();

      client.ToggleStatus();
      client.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public void ToggleStatus_UpdatesTimestamp()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      var originalUpdatedAt = client.UpdatedAt;

      Thread.Sleep(10);
      client.ToggleStatus();

      client.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void AddContract_ToActiveClient_AddsContractSuccessfully()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      var contract = new ContractModel(
        client.Id,
        "Website Redesign",
        "Complete redesign",
        ContractType.FixedPrice
      );

      client.AddContract(contract);

      client.Contracts.Should().ContainSingle();
      client.Contracts[0].Should().Be(contract);
    }

    [Fact]
    public void AddContract_ToInactiveClient_ThrowsInvalidOperationException()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      client.ToggleStatus(); // Make inactive
      var contract = new ContractModel(
        client.Id,
        "Website Redesign",
        "Complete redesign",
        ContractType.FixedPrice
      );

      var act = () => client.AddContract(contract);

      act.Should().Throw<InvalidOperationException>()
        .WithMessage("Cannot add contracts to an inactive client");
    }

    [Fact]
    public void AddContract_UpdatesTimestamp()
    {
      var client = new ClientModel("Acme Corp", "contact@acme.com");
      var contract = new ContractModel(
        client.Id,
        "Website Redesign",
        "Complete redesign",
        ContractType.FixedPrice
      );
      var originalUpdatedAt = client.UpdatedAt;

      Thread.Sleep(10);
      client.AddContract(contract);

      client.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
  }
}
