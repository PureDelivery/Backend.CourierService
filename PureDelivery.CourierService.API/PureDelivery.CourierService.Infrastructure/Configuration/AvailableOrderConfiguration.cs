using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Infrastructure.Configuration;

public class AvailableOrderConfiguration : IEntityTypeConfiguration<AvailableOrder>
{
    public void Configure(EntityTypeBuilder<AvailableOrder> builder)
    {
        builder.ToTable("AvailableOrders");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.OrderId).IsUnique();

        builder.Property(e => e.OrderNumber).HasMaxLength(20);
        builder.Property(e => e.RestaurantName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.RestaurantAddress).HasMaxLength(500);
        builder.Property(e => e.RestaurantCity).HasMaxLength(100);
        builder.Property(e => e.DeliveryAddress).HasMaxLength(500);
        builder.Property(e => e.DeliveryCity).HasMaxLength(100);
        builder.Property(e => e.CustomerName).HasMaxLength(200);
        builder.Property(e => e.DeliveryFee).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.DeliveryLatitude).HasPrecision(18, 6);
        builder.Property(e => e.DeliveryLongitude).HasPrecision(18, 6);
        builder.Property(e => e.RestaurantLatitude).HasPrecision(18, 6);
        builder.Property(e => e.RestaurantLongitude).HasPrecision(18, 6);
    }
}
