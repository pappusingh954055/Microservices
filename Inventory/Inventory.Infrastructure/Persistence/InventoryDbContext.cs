using Inventory.Domain.Entities;
using Inventory.Domain.PriceLists;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext,
    Application.Common.Interfaces.IInventoryDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>(); 
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();  
    
    public DbSet<GRNHeader> GRNHeaders => Set<GRNHeader>(); 

    public DbSet<GRNDetail> GRNDetails => Set<GRNDetail>(); 
   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        // PurchaseOrder Configuration
        modelBuilder.Entity<PurchaseOrder>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PoNumber).IsRequired().HasMaxLength(50); //

            // Mapping Private Field for DDD
            builder.Metadata.FindNavigation(nameof(PurchaseOrder.Items))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);

            // One-to-Many Relationship
            builder.HasMany(x => x.Items)
                   .WithOne()
                   .HasForeignKey(x => x.PurchaseOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseOrderItem Configuration
        modelBuilder.Entity<PurchaseOrderItem>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Rate).HasPrecision(18, 2); //
            builder.Property(x => x.Total).HasPrecision(18, 2); //
        });
    }
}
