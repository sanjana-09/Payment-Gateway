using PaymentGateway.Api.Application.Common;

namespace PaymentGateway.Api.Application.Commands;

public class PostPaymentResponse
{
    public Guid Id { get; set; }
    public PaymentStatus StatusCode { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }

    public string Status => StatusCode.ToString();
}
