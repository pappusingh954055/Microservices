using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    internal sealed class PurchaseOrderConfiguration
        : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("PurchaseOrders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SupplierId)
                   .IsRequired();

            builder.Property(x => x.PoDate)
                   .IsRequired();

        

            // 🔗 One PurchaseOrder → Many Items
            builder.HasMany(x => x.Items)
                   .WithOne(x => x.PurchaseOrder)
                   .HasForeignKey(x => x.PurchaseOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
