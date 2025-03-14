﻿using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;

namespace PaymentGateway.Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CreatePaymentController : Controller
{
    private readonly IValidator<CreatePaymentCommand> _validator;
    private readonly IMediator _mediator;

    public CreatePaymentController(IValidator<CreatePaymentCommand> validator, IMediator mediator)
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
            return RejectedPaymentResponse(validationResult, command.Id);
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