using System.Net;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.DTOs.Enums;
using PaymentGateway.Api.Application.DTOs.Requests;
using PaymentGateway.Api.Application.DTOs.Responses;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IValidator<PostPaymentRequest> _validator;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentsController(IPaymentService paymentService,
        IValidator<PostPaymentRequest> validator,
        IPaymentsRepository paymentsRepository, 
        IHttpClientFactory httpClientFactory)
    {
        _paymentService = paymentService;
        _validator = validator;
        _paymentsRepository = paymentsRepository;
        _httpClientFactory = httpClientFactory; }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        return (payment != null) ? new OkObjectResult(payment): new NotFoundObjectResult(payment);
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