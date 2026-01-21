using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScopeTrack.API;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Tests.API
{
  public sealed class TestApiFactory(string dbName) : WebApplicationFactory<Program>
  {
    private readonly string _dbName = dbName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Test");
      builder.ConfigureTestServices(services =>
      {
        var descriptor = services.SingleOrDefault(d =>
          d.ServiceType == typeof(DbContextOptions<ScopeTrackDbContext>)
        );
        if (descriptor != null) services.Remove(descriptor);

        services.AddDbContext<ScopeTrackDbContext>(options =>
        {
          options.UseInMemoryDatabase(_dbName);
        });
      });
    }
  }
}
