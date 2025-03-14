using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application.Queries;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GetPaymentController : Controller
{
    private readonly IMediator _mediator;

    public GetPaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetPaymentAsync(Guid id)
    {
        var getPaymentQuery = new GetPaymentQuery(id);
        var paymentResponse = await _mediator.Send(getPaymentQuery);

        if (paymentResponse is null) return new NotFoundResult();

        return new OkObjectResult(paymentResponse);
    }
}