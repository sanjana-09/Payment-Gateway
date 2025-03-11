using MediatR;

using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;

namespace PaymentGateway.Api.Application.Commands
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
    {
        private readonly IBankSimulator _bankSimulator;
        private readonly IPaymentsRepository _paymentsRepository;

        public CreatePaymentCommandHandler(IBankSimulator bankSimulator, IPaymentsRepository paymentsRepository)
        {
            _bankSimulator = bankSimulator;
            _paymentsRepository = paymentsRepository;
        }
        public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {

            var bankPostPaymentRequest = CreateBankPaymentRequest(createPaymentCommand);

            var bankResponse = await _bankSimulator.ProcessPaymentAsync(bankPostPaymentRequest);

            var payment = CreatePayment(createPaymentCommand);

            if (bankResponse is not null && bankResponse.Authorized)
                payment.Authorized();
            else
                payment.Declined();

            _paymentsRepository.Add(payment);

            return CreatePostPaymentResponse(payment);
        }

        private BankRequest CreateBankPaymentRequest(CreatePaymentCommand createPaymentCommand)
        {
            return new BankRequest(
                Card_Number: createPaymentCommand.CardNumber,
                Expiry_Date: $"{createPaymentCommand.ExpiryMonth}/{createPaymentCommand.ExpiryYear}",
                Currency: createPaymentCommand.Currency,
                Amount: createPaymentCommand.Amount,
                Cvv: createPaymentCommand.Cvv
            );

        }

        private Payment CreatePayment(CreatePaymentCommand createPaymentCommand)
        {
            return new Payment()
            {
                Id = Guid.NewGuid(),
                Amount = createPaymentCommand.Amount,
                Currency = createPaymentCommand.Currency,
                ExpiryMonth = createPaymentCommand.ExpiryMonth,
                ExpiryYear = createPaymentCommand.ExpiryYear,
                CardNumberLastFour = MaskCardNumber(createPaymentCommand),
                Status = Payment.PaymentStatus.Pending
            };
        }

        private CreatePaymentResponse CreatePostPaymentResponse(Payment payment)
        {
            return new CreatePaymentResponse(
                Id: payment.Id,
                PaymentStatusCode: payment.Status switch
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

        private static string MaskCardNumber(CreatePaymentCommand createPaymentCommand)
        {
            var lastFour = int.Parse(createPaymentCommand.CardNumber.Substring(createPaymentCommand.CardNumber.Length - 4));
            return $"**** **** **** {lastFour}";
        }
    }
}
