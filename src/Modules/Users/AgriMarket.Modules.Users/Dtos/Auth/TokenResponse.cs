namespace AgriMarket.Modules.Users.Dtos.Auth;

internal sealed class TokenResponse
{
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
}
