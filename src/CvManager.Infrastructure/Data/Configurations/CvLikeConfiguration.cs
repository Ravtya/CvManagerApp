using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class CvLikeConfiguration : IEntityTypeConfiguration<CvLike>
{
    public void Configure(EntityTypeBuilder<CvLike> builder)
    {
        builder.ToTable("CvLikes");

        builder.HasKey(x => new { x.CvId, x.UserId });

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(FieldLengths.UserId);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
