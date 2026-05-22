using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Shared.Web;

/// <summary>
/// Shared base for module API controllers. Exposes the JWT identity helpers
/// every module controller relies on. Lives in the shared kernel so each
/// module assembly can derive from it without referencing the bootstrapper.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>The JWT <c>sub</c> claim — the authenticated AppUser id.</summary>
    private const string SubClaimType = "sub";

    /// <summary>The custom <c>profileId</c> claim — the caller's UserProfile id.</summary>
    private const string ProfileIdClaimType = "profileId";

    protected bool TryGetUserId(out Guid userId)
    {
        var sub = User.FindFirst(SubClaimType)?.Value;
        return Guid.TryParse(sub, out userId);
    }

    protected bool TryGetProfileId(out Guid profileId)
    {
        var value = User.FindFirst(ProfileIdClaimType)?.Value;
        return Guid.TryParse(value, out profileId);
    }

    protected Guid? GetCallerUserId()
    {
        var sub = User.FindFirst(SubClaimType)?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}
