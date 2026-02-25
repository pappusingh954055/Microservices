using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryTransactionConfiguration
    : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(x => x.TransactionType)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.ReferenceId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.WarehouseId)
               .IsRequired(false);

        builder.Property(x => x.RackId)
               .IsRequired(false);

        builder.Property(x => x.CreatedOn)
               .IsRequired();
    }
}
