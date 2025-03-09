using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.DTOs.Enums;
using PaymentGateway.Api.Application.DTOs.Requests;
using PaymentGateway.Api.Application.DTOs.Responses;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IValidator<PostPaymentRequest> _validator;
    private readonly IPaymentsRepository _paymentsRepository;

    public PaymentsController(IPaymentService paymentService,
        IValidator<PostPaymentRequest> validator,
        IPaymentsRepository paymentsRepository)
    {
        _paymentService = paymentService;
        _validator = validator;
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentResponse = await _paymentService.Get(id);
        var payment = _paymentsRepository.Get(id);

        if (payment is null) return new NotFoundResult();

        return new OkObjectResult(paymentResponse);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePaymentAsync([FromBody] PostPaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return RejectedPaymentResponse(validationResult);
        }

        var response = await _paymentService.ProcessPaymentAsync(request);

        return new OkObjectResult(response);
    }

    private ActionResult<PaymentResponse> RejectedPaymentResponse(ValidationResult validationResult)
    {
        return new BadRequestObjectResult(new
        { 
            Response = new PaymentResponse(){Status = PaymentStatus.Rejected}, 
            Errors = validationResult.Errors.Select(e => e.ErrorMessage) 
        });
    }
}