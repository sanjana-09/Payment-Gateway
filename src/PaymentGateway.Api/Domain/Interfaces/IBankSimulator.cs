using PaymentGateway.Api.Domain.Requests;
using PaymentGateway.Api.Domain.Responses;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Domain.Interfaces;

public interface IBankSimulator
{
    Task<BankResponse?> ProcessPaymentAsync(BankPostPaymentRequest bankPostPaymentRequest);
}