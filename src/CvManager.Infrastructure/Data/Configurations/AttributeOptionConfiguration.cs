using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class AttributeOptionConfiguration : IEntityTypeConfiguration<AttributeOption>
{
    public void Configure(EntityTypeBuilder<AttributeOption> builder)
    {
        builder.ToTable("AttributeOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(FieldLengths.OptionValue);

        builder.HasIndex(x => new { x.AttributeDefinitionId, x.Value })
            .IsUnique();

        builder.HasMany(x => x.ProfileValues)
            .WithOne(x => x.DropdownOption)
            .HasForeignKey(x => x.DropdownOptionId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
