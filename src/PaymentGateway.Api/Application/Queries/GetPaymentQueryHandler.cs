using MediatR;

using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Application.Queries
{
    public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, GetPaymentResponse?>
    {
        private readonly IPaymentsRepository _paymentsRepository;

        public GetPaymentQueryHandler(IPaymentsRepository paymentsRepository)
        {
            _paymentsRepository = paymentsRepository;
        }

        public async Task<GetPaymentResponse?> Handle(GetPaymentQuery query, CancellationToken cancellationToken)
        {
            GetPaymentResponse? paymentResponse = null;

            var payment = await _paymentsRepository.GetAsync(query.Id);

            return payment is null ? paymentResponse : CreateGetPaymentResponse(payment);
        }

        private GetPaymentResponse CreateGetPaymentResponse(Payment payment)
        {
            return new GetPaymentResponse(
                Id: payment.Id,
                Status: payment.Status switch
                {
                    Payment.PaymentStatus.Authorized => PaymentStatus.Authorized,
                    Payment.PaymentStatus.Declined => PaymentStatus.Declined,
                    _ => PaymentStatus.Declined
                },
                CardNumberLastFour: payment.CardNumberLastFour,
                ExpiryMonth: payment.ExpiryMonth,
                ExpiryYear: payment.ExpiryYear,
                Currency: payment.Currency,
                Amount: payment.Amount
            );
        }
    }
}
