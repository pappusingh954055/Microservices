using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public sealed class SubcategoryConfiguration
    : IEntityTypeConfiguration<Subcategory>
{
    public void Configure(EntityTypeBuilder<Subcategory> builder)
    {
        builder.ToTable("Subcategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubcategoryCode)
               .IsRequired(false)
               .HasMaxLength(50);

        builder.Property(x => x.SubcategoryName)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.DefaultGst)
               .HasPrecision(5, 2)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.IsActive)
               .IsRequired();

        builder.Property(x => x.CategoryId)
               .IsRequired();

        // FK → Category
        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
