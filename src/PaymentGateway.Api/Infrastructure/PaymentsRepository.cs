using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Infrastructure;

public class PaymentsRepository : IPaymentsRepository

{
    public List<Payment?> Payments = new();
    
    public async Task AddAsync(Payment? payment)
    {
       Payments.Add(payment);

        await Task.CompletedTask;
    }

    public async Task<Payment?> GetAsync(Guid id)
    { 
        return await Task.FromResult(Payments.FirstOrDefault(p => p.Id == id));
    }
}