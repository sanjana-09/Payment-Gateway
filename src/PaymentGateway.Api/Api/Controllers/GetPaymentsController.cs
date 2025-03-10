using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.Queries;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GetPaymentsController : Controller
{
    private readonly IPaymentService _paymentService;

    public GetPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentResponse = await _paymentService.Get(id);

        if (paymentResponse is null) return new NotFoundResult();

        return new OkObjectResult(paymentResponse);
    }
}