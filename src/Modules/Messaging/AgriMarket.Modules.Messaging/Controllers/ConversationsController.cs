using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Messaging.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/conversations")]
[Authorize]
internal sealed class ConversationsController(IMessagingService messagingService) : ApiControllerBase
{
    private readonly IMessagingService _messagingService = messagingService;

    [HttpPost]
    [ProducesResponseType(typeof(ConversationDto), 201)]
    [ProducesResponseType(typeof(ConversationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateConversationDto req)
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        try
        {
            var (conversation, isNew) = await _messagingService.CreateConversationAsync(callerProfileId, req);

            if (isNew)
                return CreatedAtAction(nameof(GetById), new { id = conversation.Id }, conversation);

            return Ok(conversation);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 400, title: "Bad Request", detail: ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ConversationSummaryDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        if (page < 1) page = 1;

        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        var result = await _messagingService.GetConversationsAsync(callerProfileId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConversationDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        if (page < 1) page = 1;

        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        try
        {
            var conversation = await _messagingService.GetConversationAsync(callerProfileId, id, page, pageSize);
            return Ok(conversation);
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

    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageDto req)
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        try
        {
            var message = await _messagingService.SendMessageAsync(callerProfileId, id, req);
            return StatusCode(201, message);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 400, title: "Bad Request", detail: ex.Message);
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

    [HttpPost("{id:guid}/read-all")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAllAsRead(Guid id)
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        try
        {
            var count = await _messagingService.MarkAllAsReadAsync(callerProfileId, id);
            return Ok(new { markedCount = count });
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

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        var result = await _messagingService.GetUnreadCountAsync(callerProfileId);
        return Ok(result);
    }
}
