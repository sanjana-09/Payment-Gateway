namespace PaymentGateway.Api.Domain.Requests;

public record BankRequest(
    string Card_Number,
    string Expiry_Date,
    string Currency,
    int Amount,
    string Cvv
);
