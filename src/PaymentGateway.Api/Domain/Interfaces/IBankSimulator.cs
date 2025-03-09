using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Domain.Interfaces;

public interface IBankSimulator
{
    Task<HttpResponseMessage?> ProcessPaymentAsync(BankPostPaymentRequest bankPostPaymentRequest);
}