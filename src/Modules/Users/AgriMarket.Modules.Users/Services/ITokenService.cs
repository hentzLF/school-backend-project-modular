using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Modules.Users.Services;

internal interface ITokenService
{
    string GenerateAccessToken(AppUser user, UserProfile profile, IEnumerable<RoleType> roles);
    string GenerateRefreshToken();
}
