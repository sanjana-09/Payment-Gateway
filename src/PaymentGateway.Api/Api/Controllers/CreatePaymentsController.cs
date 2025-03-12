using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CreatePaymentsController : Controller
{
    private readonly IValidator<CreatePaymentCommand> _validator;
    private readonly IMediator _mediator;

    public CreatePaymentsController(IValidator<CreatePaymentCommand> validator, IMediator mediator)
    {
        _validator = validator;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> CreatePaymentAsync([FromBody] CreatePaymentCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return RejectedPaymentResponse(validationResult);
        }

        var paymentResponse = await _mediator.Send(command);

        return paymentResponse is null ? ErrorResponse() : new OkObjectResult(paymentResponse);
    }

    private ActionResult ErrorResponse()
    {
        return StatusCode(500, new { message = "Internal server error", 
            details = "Something went wrong and the payment could not be processed. Please try again later." });
    }

    private ActionResult RejectedPaymentResponse(ValidationResult validationResult)
    {
        var errors = validationResult.Errors.Select(e =>  e.ErrorMessage);
        var rejectedPaymentResponse = new RejectedPaymentResponse(Guid.NewGuid(), errors);

        return new BadRequestObjectResult(rejectedPaymentResponse);
    }
}