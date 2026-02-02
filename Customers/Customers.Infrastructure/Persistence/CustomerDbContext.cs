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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Apply Fluent Configurations
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        }

    }
}
