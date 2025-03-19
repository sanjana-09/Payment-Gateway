namespace PaymentGateway.Api.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = null!;
    public int Amount { get; set; }
    public string? Reason { get; set; }

    public enum PaymentStatus
    {
        Authorized = 1,
        Declined = 2,
        Pending = 3
    }

    public void Authorized() => Status = PaymentStatus.Authorized;
    public void Declined() => Status = PaymentStatus.Declined;
}
