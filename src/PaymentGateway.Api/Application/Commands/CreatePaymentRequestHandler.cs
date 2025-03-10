using MediatR;

using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;

namespace PaymentGateway.Api.Application.Commands
{
    public class CreatePaymentRequestHandler : IRequestHandler<CreatePaymentRequest, PostPaymentResponse>
    {
        private readonly IBankSimulator _bankSimulator;
        private readonly IPaymentsRepository _paymentsRepository;

        public CreatePaymentRequestHandler(IBankSimulator bankSimulator, IPaymentsRepository paymentsRepository)
        {
            _bankSimulator = bankSimulator;
            _paymentsRepository = paymentsRepository;
        }
        public async Task<PostPaymentResponse> Handle(CreatePaymentRequest createPaymentRequest, CancellationToken cancellationToken)
        {

            var bankPostPaymentRequest = CreateBankPaymentRequest(createPaymentRequest);

            var bankResponse = await _bankSimulator.ProcessPaymentAsync(bankPostPaymentRequest);

            var payment = CreatePayment(createPaymentRequest);

            if (bankResponse is not null && bankResponse.Authorized)
                payment.Authorized();
            else
                payment.Declined();

            _paymentsRepository.Add(payment);

            return CreatePostPaymentResponse(payment);
        }

        private BankRequest CreateBankPaymentRequest(CreatePaymentRequest createPaymentRequest)
        {
            return new BankRequest()
            {
                Amount = createPaymentRequest.Amount,
                Card_Number = createPaymentRequest.CardNumber,
                Currency = createPaymentRequest.Currency,
                Cvv = createPaymentRequest.Cvv,
                Expiry_Date = $"{createPaymentRequest.ExpiryMonth}/{createPaymentRequest.ExpiryYear}"
            };
        }

        private Payment CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            return new Payment()
            {
                Id = Guid.NewGuid(),
                Amount = createPaymentRequest.Amount,
                Currency = createPaymentRequest.Currency,
                ExpiryMonth = createPaymentRequest.ExpiryMonth,
                ExpiryYear = createPaymentRequest.ExpiryYear,
                CardNumberLastFour = MaskCardNumber(createPaymentRequest),
                Status = Payment.PaymentStatus.Pending
            };
        }

        private PostPaymentResponse CreatePostPaymentResponse(Payment payment)
        {
            return new
                PostPaymentResponse
                {
                    Amount = payment.Amount,
                    CardNumberLastFour = payment.CardNumberLastFour,
                    Currency = payment.Currency,
                    ExpiryMonth = payment.ExpiryMonth,
                    ExpiryYear = payment.ExpiryYear,
                    Id = payment.Id,
                    StatusCode = payment.Status switch
                    {
                        Payment.PaymentStatus.Authorized => PaymentStatus.Authorized,
                        Payment.PaymentStatus.Declined => PaymentStatus.Declined,
                        _ => PaymentStatus.Declined
                    }
                };
        }

        private static string MaskCardNumber(CreatePaymentRequest createPaymentRequest)
        {
            var lastFour = int.Parse(createPaymentRequest.CardNumber.Substring(createPaymentRequest.CardNumber.Length - 4));
            return $"**** **** **** {lastFour}";
        }
    }
}
