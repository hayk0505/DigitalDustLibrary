using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<ReviewNote> ReviewNotes => Set<ReviewNote>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AuthorApplication> AuthorApplications => Set<AuthorApplication>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<InviteToken> InviteTokens => Set<InviteToken>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<ActivityLogEntry> ActivityLog => Set<ActivityLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Handle).IsUnique();
            e.Property(u => u.Name).HasMaxLength(200);
            e.Property(u => u.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.Property(p => p.Title).HasMaxLength(300);
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.FeaturedImage)
                .WithMany()
                .HasForeignKey(p => p.FeaturedImageId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReviewNote>(e =>
        {
            e.HasOne(r => r.Post)
                .WithMany(p => p.ReviewNotes)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MediaAsset>(e =>
        {
            e.HasOne(m => m.UploadedBy)
                .WithMany()
                .HasForeignKey(m => m.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
        });

        modelBuilder.Entity<AuthorApplication>(e =>
        {
            e.HasOne(a => a.ReviewedByUser)
                .WithMany()
                .HasForeignKey(a => a.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(r => r.TokenHash).IsUnique();
            e.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InviteToken>(e =>
        {
            e.HasIndex(i => i.TokenHash).IsUnique();
            e.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLogEntry>(e =>
        {
            e.HasOne(a => a.Actor)
                .WithMany()
                .HasForeignKey(a => a.ActorId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
