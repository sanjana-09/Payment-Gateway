using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Application.Queries;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Application
{
    public interface IPaymentService
    {
        Task<GetPaymentResponse?> Get(Guid id);
    }
    public class PaymentService: IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository;
        public PaymentService(IPaymentsRepository paymentsRepository)
        {
            _paymentsRepository = paymentsRepository;
        }

       
        public async Task<GetPaymentResponse?> Get(Guid id)
        {
            GetPaymentResponse? paymentResponse = null;

            var payment = _paymentsRepository.Get(id);

            if (payment is null)
            {
                return await Task.FromResult(paymentResponse) ;
            }

            return CreateGetPaymentResponse(payment);

        }

        private GetPaymentResponse CreateGetPaymentResponse(Payment payment)
        {
            return new GetPaymentResponse()
            {
                Amount = payment.Amount,
                CardNumberLastFour = payment.CardNumberLastFour,
                Currency = payment.Currency,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Id = payment.Id,
                Status = payment.Status switch
                {
                    Payment.PaymentStatus.Authorized => PaymentStatus.Authorized,
                    Payment.PaymentStatus.Declined => PaymentStatus.Declined,
                    _ => PaymentStatus.Declined
                }
            };
        }
    }
}
