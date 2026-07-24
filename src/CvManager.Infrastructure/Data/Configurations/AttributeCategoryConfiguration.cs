using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class AttributeCategoryConfiguration : IEntityTypeConfiguration<AttributeCategory>
{
    public void Configure(EntityTypeBuilder<AttributeCategory> builder)
    {
        builder.ToTable("AttributeCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(FieldLengths.CategoryName);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
