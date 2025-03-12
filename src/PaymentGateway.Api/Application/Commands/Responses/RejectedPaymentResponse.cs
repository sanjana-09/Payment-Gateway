using PaymentGateway.Api.Application.Common;

namespace PaymentGateway.Api.Application.Commands.Responses;

public record RejectedPaymentResponse(Guid Id, IEnumerable<string> Errors)
{
    public PaymentStatus PaymentStatusCode => PaymentStatus.Rejected;
    public string Status => PaymentStatusCode.ToString();
}