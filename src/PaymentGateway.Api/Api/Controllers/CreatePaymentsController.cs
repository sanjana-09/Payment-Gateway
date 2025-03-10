using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Common;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CreatePaymentsController : Controller
{
    private readonly IValidator<CreatePaymentRequest> _validator;
    private readonly IMediator _mediator;

    public CreatePaymentsController(IValidator<CreatePaymentRequest> validator, IMediator mediator)
    {
        _validator = validator;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> CreatePaymentAsync([FromBody] CreatePaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return RejectedPaymentResponse(validationResult);
        }

        var response = await _mediator.Send(request);

        return new OkObjectResult(response);
    }

    private ActionResult<PostPaymentResponse> RejectedPaymentResponse(ValidationResult validationResult)
    {
        return new BadRequestObjectResult(new
        { 
            Response = new PostPaymentResponse(){StatusCode = PaymentStatus.Rejected}, 
            Errors = validationResult.Errors.Select(e => e.ErrorMessage) 
        });
    }
}