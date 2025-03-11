using PaymentGateway.Api.Domain.Entities;

namespace PaymentGateway.Api.Domain.Interfaces;

public interface IPaymentsRepository
{
    void Add(Payment? payment);
    Payment? Get(Guid id);
}