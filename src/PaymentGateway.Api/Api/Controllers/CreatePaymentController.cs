using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Application.Queries;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CreatePaymentController : Controller
{
    private readonly IValidator<CreatePaymentCommand> _validator;
    private readonly IMediator _mediator;
    private readonly ILogger<CreatePaymentController> _logger;

    public CreatePaymentController(IValidator<CreatePaymentCommand> validator, IMediator mediator, ILogger<CreatePaymentController> logger)
    {
        _validator = validator;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreatePaymentAsync([FromBody] CreatePaymentCommand command)
    {
        _logger.LogInformation($"CreatePayment request received for Id: {command.Id}");

        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return RejectedPaymentResponse(validationResult, command.Id);
        }

        var existingPayment = await _mediator.Send(new GetPaymentQuery(command.Id));
        if (existingPayment is not null)
        {
            _logger.LogInformation($"PaymentStatus with Id: {command.Id} already exists. Returning existing payment.");
            return new OkObjectResult(existingPayment);
        }

        var paymentResponse = await _mediator.Send(command);

        return paymentResponse is null ? ErrorResponse() : new OkObjectResult(paymentResponse);
    }

    private ActionResult ErrorResponse()
    {
        return StatusCode(500, new { message = "Internal server error", 
            details = "Something went wrong and the payment could not be processed. Please try again later." });
    }

    private ActionResult RejectedPaymentResponse(ValidationResult validationResult, Guid id)
    {
        var errors = validationResult.Errors.Select(e =>  e.ErrorMessage);
        var rejectedPaymentResponse = new RejectedPaymentResponse(id, errors);

        return new BadRequestObjectResult(rejectedPaymentResponse);
    }
}