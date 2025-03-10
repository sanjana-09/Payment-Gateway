using PaymentGateway.Api.Application.Common;

namespace PaymentGateway.Api.Application.Queries;

public class GetPaymentResponse
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}