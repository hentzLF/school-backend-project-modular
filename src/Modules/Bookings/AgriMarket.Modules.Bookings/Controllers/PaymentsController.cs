using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Bookings.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/payments")]
internal sealed class PaymentsController(IClientPaymentService paymentService) : ApiControllerBase
{
    private readonly IClientPaymentService _paymentService = paymentService;

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PaymentReceiptDto), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Pay([FromBody] PayRequest request)
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        try
        {
            var receipt = await _paymentService.PayAsync(callerProfileId, request);
            return StatusCode(201, receipt);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 422, title: "Unprocessable Entity", detail: ex.Message);
        }
    }

    [Authorize]
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<PaymentHistoryItemDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetHistory()
    {
        if (!TryGetProfileId(out var callerProfileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        var history = await _paymentService.GetHistoryAsync(callerProfileId);
        return Ok(history);
    }
}
