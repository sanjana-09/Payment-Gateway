namespace PaymentGateway.Api.Domain.Responses;

public record BankResponse(
    bool Authorized,
    string Authorization_Code
);
