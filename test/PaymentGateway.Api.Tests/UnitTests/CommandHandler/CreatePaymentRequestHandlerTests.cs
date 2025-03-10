using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;
using PaymentGateway.Api.Domain.Responses;

namespace PaymentGateway.Api.Tests.UnitTests.CommandHandler;

[TestFixture]
public class CreatePaymentRequestHandlerTests
{
    private IPaymentsRepository _paymentsRepository;
    private IBankSimulator _bankSimulator;
    private CreatePaymentRequest _createPaymentRequest;
    private CreatePaymentRequestHandler _handler;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _bankSimulator = A.Fake<IBankSimulator>();
        _paymentsRepository = A.Fake<IPaymentsRepository>();

        _handler = new CreatePaymentRequestHandler(_bankSimulator, _paymentsRepository);

        _fixture = new Fixture();
        _createPaymentRequest = _fixture.Build<CreatePaymentRequest>()
            .With(x => x.CardNumber, "1234567891234567")
            .Create();
    }

    [Test]
    public async Task Return_authorized_response_when_bank_authorizes_payment()
    {
        // Arrange
        var bankResponse = new BankResponse() { Authorized = true, Authorization_Code = "auth_code"};

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(_createPaymentRequest, default);

        // Assert
        Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentRequest);

        Then_the_payment_is_persisted(Payment.PaymentStatus.Authorized);

        Then_the_response_contains_the_expected_information(response, _createPaymentRequest, PaymentStatus.Authorized);

    }

    [Test]
    public async Task Return_declined_response_when_bank_does_not_authorizes_payment()
    {
        // Arrange

        var bankResponse = new BankResponse() { Authorized = false, Authorization_Code = null };

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(_createPaymentRequest, default);

        // Assert
        Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentRequest);

        Then_the_payment_is_persisted(Payment.PaymentStatus.Declined);

        Then_the_response_contains_the_expected_information(response, _createPaymentRequest, PaymentStatus.Declined);
    }

    [Test]
    public async Task Return_declined_response_when_bank_response_is_null()
    {
        // Arrange

        BankResponse bankResponse = null;

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(_createPaymentRequest, default);

        // Assert
        Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentRequest);

        Then_the_payment_is_persisted(Payment.PaymentStatus.Declined);

        Then_the_response_contains_the_expected_information(response, _createPaymentRequest, PaymentStatus.Declined);
    }

    
    private void Then_a_request_is_made_to_the_bank_with_expected_information(CreatePaymentRequest request)
    {
        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>.That.Matches(br =>
                br.Amount == request.Amount
                && br.Card_Number == request.CardNumber
                && br.Currency == request.Currency
                && br.Cvv == request.Cvv
                && br.Expiry_Date == $"{request.ExpiryMonth}/{request.ExpiryYear}")))
            .MustHaveHappenedOnceExactly();
    }

    private void Then_the_payment_is_persisted(Payment.PaymentStatus paymentStatus)
    {
        A.CallTo(() =>
                _paymentsRepository.Add(A<Payment>.That.Matches(p => p.Status == paymentStatus)))
            .MustHaveHappenedOnceExactly();
    }

    private void Then_the_response_contains_the_expected_information(PostPaymentResponse response,
        CreatePaymentRequest request, PaymentStatus paymentStatus)
    {
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Status, Is.EqualTo(paymentStatus));
            Assert.That(response.Amount, Is.EqualTo(request.Amount));
            Assert.That(response.Currency, Is.EqualTo(request.Currency));
            Assert.That(response.CardNumberLastFour, Is.EqualTo("**** **** **** 4567"));
            Assert.That(response.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
            Assert.That(response.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        });
    }

}