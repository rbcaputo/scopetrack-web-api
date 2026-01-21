using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Infrastructure.Data.Configurations
{
  public sealed class ContractConfiguration : IEntityTypeConfiguration<ContractModel>
  {
    public void Configure(EntityTypeBuilder<ContractModel> builder)
    {
      builder.ToTable("Contracts");

      builder.HasKey(c => c.Id);

      builder.Property(c => c.ClientId)
        .IsRequired();

      builder.Property(c => c.Title)
        .IsRequired()
        .HasMaxLength(200);

      builder.Property(c => c.Description)
        .HasMaxLength(1000);

      builder.Property(c => c.Type)
        .IsRequired()
        .HasConversion<int>();

      builder.Property(c => c.Status)
        .IsRequired()
        .HasConversion<int>();

      builder.Property(c => c.CreatedAt)
        .IsRequired();

      builder.Property(c => c.UpdatedAt)
        .IsRequired();

      builder.HasMany(c => c.Deliverables)
        .WithOne()
        .HasForeignKey(deliverable => deliverable.ContractId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
