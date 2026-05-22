using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;
using AgriMarket.Modules.Users.Security;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Users.Persistence.Seeding;

/// <summary>
/// Runtime seeder for the Users module. Account passwords are BCrypt-hashed, so
/// the default admin/client accounts cannot be expressed as static migration
/// data — they are inserted on first startup instead.
/// </summary>
internal static class UsersDbSeeder
{
    public static async Task SeedAsync(
        UsersDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        await SeedUserIfMissingAsync(
            context, passwordHasher,
            "admin@agrimarket.ee", "Admin123!", "Admin", "AgriMarket", RoleType.Admin,
            cancellationToken);

        await SeedUserIfMissingAsync(
            context, passwordHasher,
            "client@agrimarket.ee", "Client123!", "Jaan", "Tamm", RoleType.Client,
            cancellationToken);
    }

    private static async Task SeedUserIfMissingAsync(
        UsersDbContext context,
        IPasswordHasher passwordHasher,
        string email,
        string password,
        string firstName,
        string lastName,
        RoleType role,
        CancellationToken cancellationToken)
    {
        if (await context.AppUsers.AnyAsync(u => u.Email == email, cancellationToken))
            return;

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow,
        };
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            AppUserId = user.Id,
        };

        context.AppUsers.Add(user);
        context.UserProfiles.Add(profile);
        context.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), AppUserId = user.Id, Role = role });

        await context.SaveChangesAsync(cancellationToken);
    }
}
