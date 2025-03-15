namespace PaymentGateway.Api.Application.Queries;

public record GetPaymentResponse(
    Guid Id,
    PaymentStatus PaymentStatusCode,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    string? Reason
)
{
    public string Status => PaymentStatusCode.ToString();
}

public enum PaymentStatus
{
    Authorized,
    Declined
}
