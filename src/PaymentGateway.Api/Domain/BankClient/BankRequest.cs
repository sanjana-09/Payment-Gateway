namespace PaymentGateway.Api.Domain.BankClient;

public record BankRequest(
    string Card_Number,
    string Expiry_Date,
    string Currency,
    int Amount,
    string Cvv
);
