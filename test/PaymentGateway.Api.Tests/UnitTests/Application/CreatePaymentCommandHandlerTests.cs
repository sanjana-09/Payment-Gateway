using AutoFixture;

using FakeItEasy;

using NUnit.Framework;

using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;
using PaymentGateway.Api.Domain.Responses;

namespace PaymentGateway.Api.Tests.UnitTests.Application;

[TestFixture]
public class CreatePaymentCommandHandlerTests
{
    private IPaymentsRepository _paymentsRepository;
    private IBankSimulator _bankSimulator;
    private CreatePaymentCommand _createPaymentCommand;
    private CreatePaymentCommandHandler _handler;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _bankSimulator = A.Fake<IBankSimulator>();
        _paymentsRepository = A.Fake<IPaymentsRepository>();

        _handler = new CreatePaymentCommandHandler(_bankSimulator, _paymentsRepository);

        _fixture = new Fixture();
        _createPaymentCommand = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.CardNumber, "1234567891234567")
            .Create();
    }

    [Test]
    public async Task Return_authorized_response_when_bank_authorizes_payment()
    {
        // Arrange
        var bankResponse = new BankResponse(Authorized:true, Authorization_Code: "auth_code");

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(_createPaymentCommand, default);

        // Assert
        Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentCommand);

        Then_the_payment_is_persisted(Payment.PaymentStatus.Authorized);

        Then_the_response_contains_the_expected_information(response, _createPaymentCommand, PaymentStatus.Authorized);

    }

    [Test]
    public async Task Return_declined_response_when_bank_does_not_authorize_payment()
    {
        // Arrange

        var bankResponse = new BankResponse(Authorized: false, Authorization_Code: null);

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(_createPaymentCommand, default);

        // Assert
        Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentCommand);

        Then_the_payment_is_persisted(Payment.PaymentStatus.Declined);

        Then_the_response_contains_the_expected_information(response, _createPaymentCommand, PaymentStatus.Declined);
    }

    [Test]
    public async Task Return_declined_response_when_bank_response_is_null()
    {
        // Arrange

        BankResponse bankResponse = null;

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(_createPaymentCommand, default);

        // Assert
        Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentCommand);

        Then_the_payment_is_persisted(Payment.PaymentStatus.Declined);

        Then_the_response_contains_the_expected_information(response, _createPaymentCommand, PaymentStatus.Declined);
    }

    
    private void Then_a_request_is_made_to_the_bank_with_expected_information(CreatePaymentCommand command)
    {
        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>.That.Matches(br =>
                br.Amount == command.Amount
                && br.Card_Number == command.CardNumber
                && br.Currency == command.Currency
                && br.Cvv == command.Cvv
                && br.Expiry_Date == $"{command.ExpiryMonth}/{command.ExpiryYear}")))
            .MustHaveHappenedOnceExactly();
    }

    private void Then_the_payment_is_persisted(Payment.PaymentStatus paymentStatus)
    {
        A.CallTo(() =>
                _paymentsRepository.Add(A<Payment>.That.Matches(p => p.Status == paymentStatus)))
            .MustHaveHappenedOnceExactly();
    }

    private void Then_the_response_contains_the_expected_information(CreatePaymentResponse response,
        CreatePaymentCommand command, PaymentStatus paymentStatus)
    {
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response.PaymentStatusCode, Is.EqualTo(paymentStatus));
            Assert.That(response.Amount, Is.EqualTo(command.Amount));
            Assert.That(response.Currency, Is.EqualTo(command.Currency));
            Assert.That(response.CardNumberLastFour, Is.EqualTo("**** **** **** 4567"));
            Assert.That(response.ExpiryMonth, Is.EqualTo(command.ExpiryMonth));
            Assert.That(response.ExpiryYear, Is.EqualTo(command.ExpiryYear));
        });
    }

}