using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Infrastructure.Data.Configurations
{
  public sealed class DeliverableConfiguration : IEntityTypeConfiguration<DeliverableModel>
  {
    public void Configure(EntityTypeBuilder<DeliverableModel> builder)
    {
      builder.ToTable("Deliverables");

      builder.HasKey(d => d.ID);

      builder.Property(d => d.ContractID)
        .IsRequired();

      builder.Property(d => d.Title)
        .IsRequired()
        .HasMaxLength(200);

      builder.Property(d => d.Description)
        .HasMaxLength(1000);

      builder.Property(d => d.Status)
        .IsRequired()
        .HasConversion<int>();

      builder.Property(d => d.DueDate)
        .IsRequired(false);

      builder.Property(d => d.CreatedAt)
        .IsRequired();

      builder.Property(d => d.UpdatedAt)
        .IsRequired();
    }
  }
}
