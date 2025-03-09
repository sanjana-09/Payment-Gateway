using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application.DTOs.Enums;
using PaymentGateway.Api.Application.DTOs.Requests;
using PaymentGateway.Api.Application.DTOs.Responses;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;

namespace PaymentGateway.Api.Application
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PostPaymentRequest postPaymentRequest);
        Task<PaymentResponse?> Get(Guid id);
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

        public async Task<PaymentResponse> ProcessPaymentAsync(PostPaymentRequest postPaymentRequest)
        {
            var bankPostPaymentRequest = CreateBankPostPaymentRequest(postPaymentRequest);

            var bankResponse = await _bankSimulator.ProcessPaymentAsync(bankPostPaymentRequest);

            var payment = CreatePayment(postPaymentRequest);

            if (bankResponse is not null && bankResponse.Authorized)
                payment.Authorized();
            else
                payment.Declined();

            _paymentsRepository.Add(payment);

            return await Task.FromResult(CreatePaymentResponse(payment));
        }

        public async Task<PaymentResponse?> Get(Guid id)
        {
            PaymentResponse? paymentResponse = null;

            var payment = _paymentsRepository.Get(id);

            if (payment is null)
            {
                return await Task.FromResult(paymentResponse) ;
            }

            paymentResponse = CreatePaymentResponse(payment);

            return await Task.FromResult(paymentResponse);
        }

        private PaymentResponse CreatePaymentResponse(Payment payment)
        {
            return new PaymentResponse
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

        private Payment CreatePayment(PostPaymentRequest postPaymentRequest)
        {
            return new Payment()
            {
                Amount = postPaymentRequest.Amount,
                Currency = postPaymentRequest.Currency,
                ExpiryMonth = postPaymentRequest.ExpiryMonth,
                ExpiryYear = postPaymentRequest.ExpiryYear,
                CardNumberLastFour = int.Parse(postPaymentRequest.CardNumber.Substring(postPaymentRequest.CardNumber.Length - 4)),
                Status = Payment.PaymentStatus.Pending
            };
        }

        private BankPostPaymentRequest CreateBankPostPaymentRequest(PostPaymentRequest postPaymentRequest)
        {
            return new BankPostPaymentRequest()
            {
                Amount = postPaymentRequest.Amount,
                Card_Number = postPaymentRequest.CardNumber,
                Currency = postPaymentRequest.Currency,
                Cvv = postPaymentRequest.Cvv,
                Expiry_Date = $"{postPaymentRequest.ExpiryMonth}/{postPaymentRequest.ExpiryYear}"
            };
        }
    }
}
