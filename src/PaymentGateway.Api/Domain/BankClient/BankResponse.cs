namespace PaymentGateway.Api.Domain.BankClient;

public record BankResponse(
    bool Authorized,
    string? Authorization_Code
)
{
    public string? Reason;

    public BankResponseStatus Status => Authorized ? BankResponseStatus.Authorized : BankResponseStatus.Declined;
}

public enum BankResponseStatus
{
    Authorized,
    Declined
}
