using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AgriMarket.Modules.Users.Services;

internal sealed class TokenService(IConfiguration config) : ITokenService
{
    public string GenerateAccessToken(AppUser user, UserProfile profile, IEnumerable<RoleType> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("profileId", profile.Id.ToString()),
            new(JwtRegisteredClaimNames.GivenName, profile.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, profile.LastName),
        };

        foreach (var role in roles)
            claims.Add(new Claim("role", role.ToString()));

        var expiryMinutes = int.Parse(config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        return CreateJwt(claims, TimeSpan.FromMinutes(expiryMinutes));
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private string CreateJwt(IEnumerable<Claim> claims, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
