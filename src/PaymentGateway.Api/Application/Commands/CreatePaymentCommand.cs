using MediatR;
using PaymentGateway.Api.Application.Commands.Responses;

namespace PaymentGateway.Api.Application.Commands;

public record CreatePaymentCommand(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    string Cvv
) : IRequest<CreatePaymentResponse>;