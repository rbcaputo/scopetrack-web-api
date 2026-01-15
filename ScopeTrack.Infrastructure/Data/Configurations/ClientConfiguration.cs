using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Infrastructure.Data.Configurations
{
  public sealed class ClientConfiguration : IEntityTypeConfiguration<ClientModel>
  {
    public void Configure(EntityTypeBuilder<ClientModel> builder)
    {
      builder.ToTable("Clients");

      builder.HasKey(c => c.ID);

      builder.Property(c => c.Name)
        .IsRequired()
        .HasMaxLength(200);

      builder.Property(c => c.ContactEmail)
        .IsRequired()
        .HasMaxLength(200);

      builder.Property(c => c.Status)
        .IsRequired()
        .HasConversion<int>();

      builder.Property(c => c.CreatedAt)
        .IsRequired();

      builder.Property(c => c.UpdatedAt)
        .IsRequired();

      builder.HasMany(typeof(ContractModel))
        .WithOne()
        .HasForeignKey("ClientID")
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
