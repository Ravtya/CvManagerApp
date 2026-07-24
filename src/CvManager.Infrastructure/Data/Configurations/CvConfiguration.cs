using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CvEntity = CvManager.Domain.Entities.Cv;

namespace CvManager.Infrastructure.Data.Configurations;

public class CvConfiguration : IEntityTypeConfiguration<CvEntity>
{
    public void Configure(EntityTypeBuilder<CvEntity> builder)
    {
        builder.ToTable("Cvs");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserProfileId, x.PositionId })
            .IsUnique();

        builder.Property(x => x.PublishedAt)
            .IsRequired();

        builder.HasMany(x => x.Likes)
            .WithOne(x => x.Cv)
            .HasForeignKey(x => x.CvId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
