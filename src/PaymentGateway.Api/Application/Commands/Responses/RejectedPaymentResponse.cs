using PaymentGateway.Api.Application.Common;

namespace PaymentGateway.Api.Application.Commands.Responses;

public record RejectedPaymentResponse(Guid PaymentId, IEnumerable<string> Errors)
{
    public PaymentStatus PaymentStatusCode => PaymentStatus.Rejected;
    public string Status => PaymentStatusCode.ToString();
}