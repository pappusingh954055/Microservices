using Microsoft.EntityFrameworkCore;
using Suppliers.Domain.Entities;

public class SupplierDbContext : DbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierPayment> SupplierPayments { get; set; }
    public DbSet<SupplierLedger> SupplierLedgers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DDD Encapsulation ke liye mapping
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(15);
        });

        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentMode).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<SupplierLedger>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
        });
    }
}