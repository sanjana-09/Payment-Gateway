//using NUnit.Framework;

//using PaymentGateway.Api.Application;
//using PaymentGateway.Api.Domain;
//using PaymentGateway.Api.Domain.Interfaces;
//using PaymentGateway.Api.Domain.Requests;
//using FakeItEasy;

//using PaymentGateway.Api.Application.Commands;
//using PaymentGateway.Api.Domain.Responses;
//using PaymentGateway.Api.Application.Common;

//namespace PaymentGateway.Api.Tests
//{
//    [TestFixture]
//    public class PaymentServiceTests
//    {
//        private IPaymentsRepository _paymentsRepository;
//        private IBankSimulator _bankSimulator;
//        private PaymentService _paymentService;

//        [SetUp]
//        public void Setup()
//        {
//            _paymentsRepository = A.Fake<IPaymentsRepository>();
//            _bankSimulator = A.Fake<IBankSimulator>();

//            _paymentService = new PaymentService(_paymentsRepository, _bankSimulator);
//        }

//        [Test]
//        public async Task ProcessPaymentAsync_ShouldReturnAuthorizedPayment_WhenBankSimulatorReturnsAuthorized()
//        {
//             Arrange
//            var postPaymentRequest = new CreatePaymentRequest
//            {
//                Amount = 100,
//                Currency = "USD",
//                ExpiryMonth = 12,
//                ExpiryYear = 2025,
//                CardNumber = "1234567812345678",
//                Cvv = "123"
//            };

//            var bankResponse = new BankResponse() { Authorized = true };
//            A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>.Ignored))
//                .Returns(Task.FromResult(bankResponse));

//             Act
//            var result = await _paymentService.ProcessPaymentAsync(postPaymentRequest);

//             Assert
//            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Authorized));
//            A.CallTo(() => _paymentsRepository.Add(A<Payment>.Ignored)).MustHaveHappenedOnceExactly();
//        }

//        [Test]
//        public async Task ProcessPaymentAsync_ShouldReturnDeclinedPayment_WhenBankSimulatorReturnsUnauthorized()
//        {
//             Arrange
//            var postPaymentRequest = new CreatePaymentRequest
//            {
//                Amount = 100,
//                Currency = "USD",
//                ExpiryMonth = 12,
//                ExpiryYear = 2025,
//                CardNumber = "1234567812345678",
//                Cvv = "123"
//            };

//            var bankResponse = new BankResponse() { Authorized = false };
//            A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>.Ignored))!
//                .Returns(Task.FromResult(bankResponse));

//             Act
//            var result = await _paymentService.ProcessPaymentAsync(postPaymentRequest);

//             Assert
//            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Declined));
//            A.CallTo(() => _paymentsRepository.Add(A<Payment>.Ignored)).MustHaveHappenedOnceExactly();
//        }

//        [Test]
//        public async Task ProcessPaymentAsync_ShouldCreatePaymentWithCorrectDetails()
//        {
//             Arrange
//            var postPaymentRequest = new CreatePaymentRequest
//            {
//                Amount = 100,
//                Currency = "USD",
//                ExpiryMonth = 12,
//                ExpiryYear = 2025,
//                CardNumber = "1234567812345678",
//                Cvv = "123"
//            };

//            var bankResponse = new BankResponse() { Authorized = true };
//            A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>.Ignored))
//                .Returns(Task.FromResult(bankResponse));

//             Act
//            var result = await _paymentService.ProcessPaymentAsync(postPaymentRequest);

//             Assert that the payment was added to the repository
//            A.CallTo(() => _paymentsRepository.Add(A<Payment>.That.Matches(p =>
//                p.Amount == postPaymentRequest.Amount &&
//                p.Currency == postPaymentRequest.Currency &&
//                p.ExpiryMonth == postPaymentRequest.ExpiryMonth &&
//                p.ExpiryYear == postPaymentRequest.ExpiryYear &&
//                p.CardNumberLastFour == 5678
//            ))).MustHaveHappenedOnceExactly();
//        }
//    }
//}

