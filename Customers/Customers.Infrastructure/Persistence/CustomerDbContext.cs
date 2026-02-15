using Customers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Persistence
{
    public class CustomerDbContext : DbContext
        
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
     : base(options) { }


        // 🔹 DbSet for Customer
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerReceipt> CustomerReceipts { get; set; }
        public DbSet<CustomerLedger> CustomerLedgers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Apply Fluent Configurations
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());

            modelBuilder.Entity<CustomerReceipt>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReceiptMode).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<CustomerLedger>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
            });
        }

    }
}
