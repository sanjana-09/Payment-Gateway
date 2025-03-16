using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application.Queries;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GetPaymentController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetPaymentController> _logger;

    public GetPaymentController(IMediator mediator, ILogger<GetPaymentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPaymentAsync(Guid id)
    {
        _logger.LogInformation($"GetPayment request received for Id: {id}.");

        var getPaymentQuery = new GetPaymentQuery(id);
        var paymentResponse = await _mediator.Send(getPaymentQuery);

        if (paymentResponse is null) return new NotFoundResult();

        return new OkObjectResult(paymentResponse);
    }
}