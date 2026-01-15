using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    internal sealed class PurchaseOrderItemConfiguration
        : IEntityTypeConfiguration<PurchaseOrderItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
        {
            builder.ToTable("PurchaseOrderItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId)
                   .IsRequired();

            builder.Property(x => x.Quantity)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.UnitPrice)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.DiscountPercent)
                   .HasPrecision(5, 2);

            builder.Property(x => x.GstPercent)
                   .HasPrecision(5, 2);

    
                   
 

        }
    }
}
