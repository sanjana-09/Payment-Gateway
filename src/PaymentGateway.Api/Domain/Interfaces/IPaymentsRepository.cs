using PaymentGateway.Api.Domain.Entities;

namespace PaymentGateway.Api.Domain.Interfaces;

public interface IPaymentsRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetAsync(Guid id);
}