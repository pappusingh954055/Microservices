using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Sku)
               .IsUnique();

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.Unit)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(x => x.DefaultGst)
               .HasPrecision(5, 2)
               .IsRequired(false);

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.IsActive)
               .IsRequired();

        builder.Property(x => x.CategoryId)
               .IsRequired();

        builder.Property(x => x.SubcategoryId)
               .IsRequired();

        // FK → Category
        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK → Subcategory
        builder.HasOne<Subcategory>()
               .WithMany()
               .HasForeignKey(x => x.SubcategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
