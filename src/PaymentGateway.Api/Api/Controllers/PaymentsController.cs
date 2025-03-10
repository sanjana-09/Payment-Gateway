using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IValidator<CreatePaymentRequest> _validator;
    private readonly IPaymentsRepository _paymentsRepository;

    public PaymentsController(IPaymentService paymentService,
        IValidator<CreatePaymentRequest> validator,
        IPaymentsRepository paymentsRepository)
    {
        _paymentService = paymentService;
        _validator = validator;
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentResponse = await _paymentService.Get(id);
        var payment = _paymentsRepository.Get(id);

        if (payment is null) return new NotFoundResult();

        return new OkObjectResult(paymentResponse);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> CreatePaymentAsync([FromBody] CreatePaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return RejectedPaymentResponse(validationResult);
        }

        var response = await _paymentService.ProcessPaymentAsync(request);

        return new OkObjectResult(response);
    }

    private ActionResult<PostPaymentResponse> RejectedPaymentResponse(ValidationResult validationResult)
    {
        return new BadRequestObjectResult(new
        { 
            Response = new PostPaymentResponse(){Status = PaymentStatus.Rejected}, 
            Errors = validationResult.Errors.Select(e => e.ErrorMessage) 
        });
    }
}