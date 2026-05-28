using AgriMarket.Modules.Users.Dtos.Auth;
using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;
using AgriMarket.Modules.Users.Persistence;
using AgriMarket.Modules.Users.Security;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgriMarket.Modules.Users.Services;

internal sealed class AuthService(
    IAppUserRepository appUsers,
    [FromKeyedServices("users")] IRepository<UserRole> userRoles,
    IRefreshTokenRepository refreshTokens,
    [FromKeyedServices("users")] IUnitOfWork uow,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    IConfiguration config,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await appUsers.AnyAsync(u => u.Email == request.Email, ct))
            throw new InvalidOperationException("Email already in use.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
        };

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            AppUserId = user.Id,
        };

        var clientRole = new UserRole
        {
            Id = Guid.NewGuid(),
            AppUserId = user.Id,
            Role = RoleType.Client,
        };

        await uow.BeginTransactionAsync(ct);
        try
        {
            appUsers.Add(user);
            userRoles.Add(clientRole);
            user.Profile = profile;
            await uow.SaveChangesAsync(ct);
            await uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await appUsers.GetByEmailWithProfilesAsync(request.Email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var profile = user.Profile
            ?? throw new InvalidOperationException("User has no profile.");
        var roles = user.Roles?.Select(r => r.Role).ToList()
            ?? throw new InvalidOperationException("User has no assigned roles.");
        var refreshToken = await IssueRefreshTokenAsync(user.Id, ct);

        return new TokenResponse
        {
            AccessToken = tokenService.GenerateAccessToken(user, profile, roles),
            RefreshToken = refreshToken,
        };
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await refreshTokens.GetByTokenWithUserAsync(refreshToken, ct);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow || stored.AppUser is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        await uow.BeginTransactionAsync(ct);
        try
        {
            stored.IsRevoked = true;

            var user = stored.AppUser;
            var profile = user.Profile
                ?? throw new InvalidOperationException("User has no profile.");
            var roles = user.Roles?.Select(r => r.Role).ToList()
                ?? throw new InvalidOperationException("User has no assigned roles.");
            var newRefreshToken = await IssueRefreshTokenAsync(user.Id, ct);

            await uow.SaveChangesAsync(ct);
            await uow.CommitTransactionAsync(ct);

            return new TokenResponse
            {
                AccessToken = tokenService.GenerateAccessToken(user, profile, roles),
                RefreshToken = newRefreshToken,
            };
        }
        catch
        {
            await uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await refreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);
        if (stored is null)
            return;

        stored.IsRevoked = true;
        await uow.SaveChangesAsync(ct);
    }

    private async Task<string> IssueRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        var token = tokenService.GenerateRefreshToken();
        var expiryDays = int.Parse(config["Jwt:RefreshTokenExpiryDays"] ?? "7");

        refreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            AppUserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
        });

        await uow.SaveChangesAsync(ct);
        return token;
    }
}
