using MediatR;
using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Domain.BankClient;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Application.Commands
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse?>
    {
        private readonly IBankClient _bankClient;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;

        public CreatePaymentCommandHandler(IBankClient bankClient, IPaymentsRepository paymentsRepository, ILogger<CreatePaymentCommandHandler> logger)
        {
            _bankClient = bankClient;
            _paymentsRepository = paymentsRepository;
            _logger = logger;
        }
        public async Task<CreatePaymentResponse?> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {

            var bankPaymentRequest = CreateBankPaymentRequest(createPaymentCommand);

            try
            {
                var bankResponse = await _bankClient.ProcessPaymentAsync(bankPaymentRequest);

                var payment = CreatePayment(createPaymentCommand);

                if (bankResponse is not null && bankResponse.Authorized)
                    payment.Authorized();
                else
                    payment.Declined();

                await _paymentsRepository.AddAsync(payment);

                return CreatePaymentResponse(payment);
            }

            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Payment processing failed with the error: {ex.Message}");
                return null;
            }

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

        private CreatePaymentResponse CreatePaymentResponse(Payment payment)
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

        private string MaskCardNumber(CreatePaymentCommand createPaymentCommand)
        {
            var lastFour = int.Parse(createPaymentCommand.CardNumber.Substring(createPaymentCommand.CardNumber.Length - 4));
            return $"**** **** **** {lastFour}";
        }
    }
}
