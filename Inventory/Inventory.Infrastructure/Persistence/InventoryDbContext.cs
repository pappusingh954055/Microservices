using Inventory.Domain.Entities;
using Inventory.Domain.PriceLists;
using Microsoft.EntityFrameworkCore;
using YourProjectNamespace.Entities;

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

    public DbSet<SaleOrder> SaleOrders { get; set; }
    public DbSet<SaleOrderItem> SaleOrderItems { get; set; }

    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }

    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }

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

        // Configuration for PurchaseReturn
        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReturnNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasDefaultValue("Draft"); // Default status set kiya hai
        });

        // Relationship: One Return has many Items [cite: 2026-02-03]
        modelBuilder.Entity<PurchaseReturnItem>()
            .HasOne(p => p.PurchaseReturn)
            .WithMany(i => i.Items)
            .HasForeignKey(p => p.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade); // Header delete toh items bhi delete

        // PurchaseOrderItem Configuration
        modelBuilder.Entity<PurchaseOrderItem>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Rate).HasPrecision(18, 2); //
            builder.Property(x => x.Total).HasPrecision(18, 2); //
        });

        // SaleOrder aur SaleOrderItem ke beech Cascade Delete configuration
        modelBuilder.Entity<SaleOrderItem>()
           .HasOne(i => i.SaleOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.SaleOrderId) 
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.NoAction); // Cascade ki jagah NoAction use karein
        });
    }
}
