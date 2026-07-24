using CvManager.Domain.Entities;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CvEntity = CvManager.Domain.Entities.Cv;

namespace CvManager.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeCategory> AttributeCategories => Set<AttributeCategory>();
    public DbSet<AttributeOption> AttributeOptions => Set<AttributeOption>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ProfileAttributeValue> ProfileAttributeValues => Set<ProfileAttributeValue>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagAssignment> TagAssignments => Set<TagAssignment>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PositionAttribute> PositionAttributes => Set<PositionAttribute>();
    public DbSet<PositionTag> PositionTags => Set<PositionTag>();
    public DbSet<CvEntity> Cvs => Set<CvEntity>();
    public DbSet<CvLike> CvLikes => Set<CvLike>();
    public DbSet<DiscussionPost> DiscussionPosts => Set<DiscussionPost>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseExceptionProcessor();
}