using Microsoft.EntityFrameworkCore;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Infrastructure.Data.Configurations;

namespace ScopeTrack.Infrastructure.Data
{
  public sealed class ScopeTrackDbContext(
    DbContextOptions<ScopeTrackDbContext> options
  ) : DbContext(options)
  {
    public DbSet<ClientModel> Clients => Set<ClientModel>();
    public DbSet<ContractModel> Contracts => Set<ContractModel>();
    public DbSet<DeliverableModel> Deliverables => Set<DeliverableModel>();
    public DbSet<ActivityLogModel> ActivityLogs => Set<ActivityLogModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfiguration(new ClientConfiguration());
      modelBuilder.ApplyConfiguration(new ContractConfiguration());
      modelBuilder.ApplyConfiguration(new DeliverableConfiguration());
      modelBuilder.ApplyConfiguration(new ActivityLogConfiguration());
    }
  }
}
