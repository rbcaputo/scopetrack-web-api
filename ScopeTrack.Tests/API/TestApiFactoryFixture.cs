namespace ScopeTrack.Tests.API
{
  public sealed class TestApiFactoryFixture : IDisposable
  {
    public TestApiFactory Factory { get; }

    public TestApiFactoryFixture()
      => Factory = new TestApiFactory($"TestDb_{Guid.NewGuid()}");

    public void Dispose()
      => Factory.Dispose();
  }
}
