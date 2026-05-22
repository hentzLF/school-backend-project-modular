using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Bookings.Controllers.Admin;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/payments")]
[Authorize(Policy = "AdminOnly")]
internal sealed class AdminPaymentsController(IPaymentService paymentService) : ApiControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Payment>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll([FromQuery] PaymentStatus? status)
    {
        var payments = await _paymentService.GetAllAsync(status);
        return Ok(payments);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Payment {id} not found.");

        return Ok(payment);
    }

    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveDisputeRequest req)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Payment {id} not found.");

        if (payment.Status != PaymentStatus.Disputed)
            return Problem(statusCode: 422, title: "Unprocessable Entity", detail: "Only disputed payments can be resolved.");

        await _paymentService.ResolveDisputeAsync(id, req.Resolution);

        var updated = await _paymentService.GetByIdAsync(id);
        return Ok(updated);
    }
}

internal sealed record ResolveDisputeRequest(PaymentResolution Resolution);
