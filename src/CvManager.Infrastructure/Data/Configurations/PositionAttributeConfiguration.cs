using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class PositionAttributeConfiguration : IEntityTypeConfiguration<PositionAttribute>
{
    public void Configure(EntityTypeBuilder<PositionAttribute> builder)
    {
        builder.ToTable("PositionAttributes");

        builder.HasKey(x => new { x.PositionId, x.AttributeDefinitionId });

        builder.Property(x => x.Operator)
            .HasConversion<string>()
            .HasMaxLength(FieldLengths.EnumLabel);

        builder.Property(x => x.ComparisonString)
            .HasMaxLength(FieldLengths.AttributeString);

        builder.Property(x => x.ComparisonNumeric)
            .HasPrecision(18, 4);

        builder.HasOne(x => x.ComparisonOption)
            .WithMany(o => o.PositionAttributes)
            .HasForeignKey(x => x.ComparisonOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
