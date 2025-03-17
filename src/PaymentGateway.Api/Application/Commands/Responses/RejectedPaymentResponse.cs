namespace PaymentGateway.Api.Application.Commands.Responses;

public record RejectedPaymentResponse(Guid Id, IEnumerable<string> Errors)
{
    public PaymentStatus PaymentStatusCode => Responses.PaymentStatus.Rejected;
    public string PaymentStatus => PaymentStatusCode.ToString();
}