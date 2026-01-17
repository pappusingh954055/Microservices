using Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoleName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.RoleName)
            .IsUnique();

        // ✅ SEED DATA
        builder.HasData(
            new Role(1, "Admin"),
            new Role(2, "Teacher"),
            new Role(3, "Student"),
            new Role(4, "Parent"),
            new Role(5, "Employee")
        );
    }
}
