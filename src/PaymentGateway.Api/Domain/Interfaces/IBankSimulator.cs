using PaymentGateway.Api.Domain.Requests;
using PaymentGateway.Api.Domain.Responses;

namespace PaymentGateway.Api.Domain.Interfaces;

public interface IBankSimulator
{
    Task<BankResponse?> ProcessPaymentAsync(BankRequest bankRequest);
}