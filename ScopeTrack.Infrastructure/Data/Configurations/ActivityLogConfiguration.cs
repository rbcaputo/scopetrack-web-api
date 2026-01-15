using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Infrastructure.Data.Configurations
{
  public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLogModel>
  {
    public void Configure(EntityTypeBuilder<ActivityLogModel> builder)
    {
      builder.ToTable("ActivityLogs");

      builder.HasKey(a => a.ID);

      builder.Property(a => a.EntityType)
        .IsRequired()
        .HasConversion<int>();

      builder.Property(a => a.EntityID)
        .IsRequired();

      builder.Property(a => a.ActivityType)
        .IsRequired()
        .HasConversion<int>();

      builder.Property(a => a.Description)
        .IsRequired()
        .HasMaxLength(500);

      builder.Property(a => a.OccurredAt)
        .IsRequired();

      builder.HasIndex(a => new { a.EntityType, a.EntityID });
      builder.HasIndex(a => a.OccurredAt);
    }
  }
}
