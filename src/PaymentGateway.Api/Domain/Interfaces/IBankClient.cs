using PaymentGateway.Api.Domain.BankClient;

namespace PaymentGateway.Api.Domain.Interfaces;

public interface IBankClient
{
    Task<BankResponse?> ProcessPaymentAsync(BankRequest bankRequest);
}