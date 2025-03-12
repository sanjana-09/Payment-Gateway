using MediatR;
namespace PaymentGateway.Api.Application.Queries;

public record GetPaymentQuery(Guid Id) : IRequest<GetPaymentResponse>;