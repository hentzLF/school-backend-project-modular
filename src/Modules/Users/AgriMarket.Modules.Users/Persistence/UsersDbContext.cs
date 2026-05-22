using AgriMarket.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Users.Persistence;

internal sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers { get; set; } = default!;
    public DbSet<UserProfile> UserProfiles { get; set; } = default!;
    public DbSet<UserRole> UserRoles { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("users");

        // AppUser: email must be unique
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // UserRole: same user cannot have the same role twice
        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.AppUserId, ur.Role })
            .IsUnique();

        // UserProfile: one profile per user (1:1)
        modelBuilder.Entity<UserProfile>()
            .HasIndex(up => up.AppUserId)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.AppUser)
            .HasForeignKey<UserProfile>(p => p.AppUserId);
    }
}
