using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(FieldLengths.Name);

        builder.Property(x => x.ShortDescription)
            .IsRequired()
            .HasMaxLength(FieldLengths.Description);

        builder.Property(x => x.AccessMode)
            .HasConversion<string>()
            .HasMaxLength(FieldLengths.EnumLabel);

        builder.Property(x => x.MaxProjectsInCv)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "simple",
                p => new { p.Title, p.ShortDescription })
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        builder.HasMany(x => x.Attributes)
            .WithOne(x => x.Position)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne(x => x.Position)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Cvs)
            .WithOne(x => x.Position)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DiscussionPosts)
            .WithOne(x => x.Position)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
