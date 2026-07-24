using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("Attributes");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(FieldLengths.Name);
        builder.Property(x => x.Description)
            .HasMaxLength(FieldLengths.Description);
        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.DataType)
            .HasConversion<string>()
            .HasMaxLength(FieldLengths.EnumLabel);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Attributes)
            .HasForeignKey(x => x.AttributeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Options)
            .WithOne(x => x.Attribute)
            .HasForeignKey(x => x.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProfileValues)
            .WithOne(x => x.Attribute)
            .HasForeignKey(x => x.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PositionAttributes)
            .WithOne(x => x.Attribute)
            .HasForeignKey(x => x.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
