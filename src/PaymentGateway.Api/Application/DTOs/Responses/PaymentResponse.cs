using PaymentGateway.Api.Application.DTOs.Enums;

namespace PaymentGateway.Api.Application.DTOs.Responses;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}
