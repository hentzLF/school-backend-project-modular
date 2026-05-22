using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Messaging.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/messages")]
[Authorize]
internal sealed class MessagesController(IMessagingService messagingService) : ApiControllerBase
{
    private readonly IMessagingService _messagingService = messagingService;

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        try
        {
            await _messagingService.MarkAsReadAsync(callerProfileId, id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
    }
}
