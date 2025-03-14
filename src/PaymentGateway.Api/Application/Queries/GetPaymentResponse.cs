namespace PaymentGateway.Api.Application.Queries;

public record GetPaymentResponse(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount
);

public enum PaymentStatus
{
    Authorized,
    Declined
}
