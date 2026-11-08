using HomeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinance.Infrastructure.Configurations;

public sealed class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.ToTable("Households", table =>
            table.HasCheckConstraint("CK_Households_Name_NotEmpty", "length(trim(\"Name\")) > 0"));
        builder.HasKey(household => household.Id);
        builder.Property(household => household.Id).ValueGeneratedNever();
        builder.Property(household => household.Name).IsRequired().HasMaxLength(200);
        builder.Property(household => household.OwnerId).IsRequired();
        builder.HasIndex(household => household.OwnerId);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(household => household.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(household => household.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("ARS");
        builder.Property(household => household.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("America/Argentina/Cordoba");
        builder.Property(household => household.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
    }
}
