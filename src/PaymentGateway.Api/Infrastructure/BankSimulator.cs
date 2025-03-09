using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Infrastructure
{
    public class BankPostPaymentRequest
    {
    }

    public class BankSimulator : IBankSimulator
    {
        public async Task<HttpResponseMessage> ProcessPaymentAsync(BankPostPaymentRequest bankPostPaymentRequest)
        {
            throw new NotImplementedException();
        }
    }
}
