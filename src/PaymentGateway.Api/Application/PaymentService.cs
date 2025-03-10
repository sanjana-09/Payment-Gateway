using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Application.Queries;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;

namespace PaymentGateway.Api.Application
{
    public interface IPaymentService
    {
        Task<PostPaymentResponse> ProcessPaymentAsync(CreatePaymentRequest createPaymentRequest);
        Task<GetPaymentResponse?> Get(Guid id);
    }
    public class PaymentService: IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IBankSimulator _bankSimulator;

        public PaymentService(IPaymentsRepository paymentsRepository, IBankSimulator bankSimulator)
        {
            _paymentsRepository = paymentsRepository;
            _bankSimulator = bankSimulator;
        }

        public async Task<PostPaymentResponse> ProcessPaymentAsync(CreatePaymentRequest createPaymentRequest)
        {
            var bankPostPaymentRequest = CreateBankPostPaymentRequest(createPaymentRequest);

            var bankResponse = await _bankSimulator.ProcessPaymentAsync(bankPostPaymentRequest);

            var payment = CreatePayment(createPaymentRequest);

            if (bankResponse is not null && bankResponse.Authorized)
                payment.Authorized();
            else
                payment.Declined();

            _paymentsRepository.Add(payment);

            return await Task.FromResult(CreatePostPaymentResponse(payment));
        }

        public async Task<GetPaymentResponse?> Get(Guid id)
        {
            GetPaymentResponse? paymentResponse = null;

            var payment = _paymentsRepository.Get(id);

            if (payment is null)
            {
                return await Task.FromResult(paymentResponse) ;
            }

            paymentResponse = CreateGetPaymentResponse(payment);

            return await Task.FromResult(paymentResponse);
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
                Status = payment.Status switch
                {
                    Payment.PaymentStatus.Authorized => PaymentStatus.Authorized,
                    Payment.PaymentStatus.Declined => PaymentStatus.Declined,
                    _ => PaymentStatus.Declined
                }
            };
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

        private Payment CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            return new Payment()
            {
                Amount = createPaymentRequest.Amount,
                Currency = createPaymentRequest.Currency,
                ExpiryMonth = createPaymentRequest.ExpiryMonth,
                ExpiryYear = createPaymentRequest.ExpiryYear,
                CardNumberLastFour = MaskCardNumber(createPaymentRequest),
                Status = Payment.PaymentStatus.Pending
            };
        }

        private static string MaskCardNumber(CreatePaymentRequest createPaymentRequest)
        {
            var lastFour = int.Parse(createPaymentRequest.CardNumber.Substring(createPaymentRequest.CardNumber.Length - 4));
            return $"**** **** **** {lastFour}";
        }

        private BankRequest CreateBankPostPaymentRequest(CreatePaymentRequest createPaymentRequest)
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
    }
}
