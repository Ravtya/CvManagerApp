using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class ProfileAttributeValueConfiguration : IEntityTypeConfiguration<ProfileAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProfileAttributeValue> builder)
    {
        builder.ToTable("ProfileAttributeValues");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserProfileId, x.AttributeDefinitionId })
            .IsUnique();

        builder.Property(x => x.StringValue)
            .HasMaxLength(FieldLengths.AttributeString);

        builder.Property(x => x.TextValue)
            .HasMaxLength(FieldLengths.Text);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(FieldLengths.Description);

        builder.Property(x => x.NumericValue)
            .HasPrecision(18, 4);

        builder.HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "simple",
                p => new { p.StringValue, p.TextValue })
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");
    }
}