using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Infrastructure.Configuration;

public class CourierAssignmentConfiguration : IEntityTypeConfiguration<CourierAssignment>
{
    public void Configure(EntityTypeBuilder<CourierAssignment> builder)
    {
        builder.ToTable("CourierAssignments");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.OrderId).IsUnique();
        builder.HasIndex(e => new { e.PickupCode, e.Status });

        builder.Property(e => e.OrderNumber).HasMaxLength(20);
        builder.Property(e => e.PickupCode).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Status).IsRequired();

        builder.HasOne(e => e.Courier)
               .WithMany()
               .HasForeignKey(e => e.CourierId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
