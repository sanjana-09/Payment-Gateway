using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Infrastructure;

public class PaymentsRepository : IPaymentsRepository

{
    public List<Payment?> Payments = new();
    
    public void Add(Payment? payment)
    {
        Payments.Add(payment);
    }

    public Payment? Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}