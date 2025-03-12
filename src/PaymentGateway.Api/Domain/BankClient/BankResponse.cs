namespace PaymentGateway.Api.Domain.BankClient;

public record BankResponse(
    bool Authorized,
    string? Authorization_Code
);
