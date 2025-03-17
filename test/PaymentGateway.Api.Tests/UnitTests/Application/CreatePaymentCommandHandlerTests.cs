using AutoFixture;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Domain.BankClient;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Tests.UnitTests.Application;

[TestFixture]
public class CreatePaymentCommandHandlerTests
{
    private IPaymentsRepository _paymentsRepository;
    private IBankClient _bankClient;
    private ILogger<CreatePaymentCommandHandler> _logger;
    private CreatePaymentCommand _createPaymentCommand;
    private CreatePaymentCommandHandler _handler;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _bankClient = A.Fake<IBankClient>();
        _paymentsRepository = A.Fake<IPaymentsRepository>();
        _logger = A.Fake<ILogger<CreatePaymentCommandHandler>>();

        _handler = new CreatePaymentCommandHandler(_bankClient, _paymentsRepository, _logger);

        _fixture = new Fixture();
        _createPaymentCommand = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.CardNumber, "1234567891234567")
            .Create();
    }

    [Test]
    public async Task Returns_authorized_response_when_bank_authorizes_payment()
    {
        // Arrange
        var bankResponse = new BankResponse(Authorized: true, Authorization_Code: "auth_code");

        A.CallTo(() => _bankClient.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var createPaymentResponse = await _handler.Handle(_createPaymentCommand, default);

        // Assert
        Assert.Multiple(() =>
        {
            Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentCommand);
            Then_the_payment_is_persisted(bankResponse, _createPaymentCommand);
            Then_the_response_contains_the_expected_information(createPaymentResponse, _createPaymentCommand, bankResponse);
        });
    }

    [Test]
    public async Task Returns_declined_response_when_bank_does_not_authorize_payment()
    {
        // Arrange
        var bankResponse = new BankResponse(Authorized: false, Authorization_Code: null){Reason = "Service Unavailable"};

        A.CallTo(() => _bankClient.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var createPaymentResponse = await _handler.Handle(_createPaymentCommand, default);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(createPaymentResponse, Is.Not.Null);
            Then_a_request_is_made_to_the_bank_with_expected_information(_createPaymentCommand);
            Then_the_payment_is_persisted(bankResponse, _createPaymentCommand);
            Then_the_response_contains_the_expected_information(createPaymentResponse, _createPaymentCommand, bankResponse);
        });
    }

    [Test]
    public async Task Returns_null_when_exception_is_thrown()
    {
        // Arrange
        A.CallTo(() => _bankClient.ProcessPaymentAsync(A<BankRequest>._)).Throws<Exception>();

        // Act
        var createPaymentResponse = await _handler.Handle(_createPaymentCommand, default);

        // Assert
        Assert.That(createPaymentResponse, Is.Null);
    }

    #region Helper methods
    private void Then_a_request_is_made_to_the_bank_with_expected_information(CreatePaymentCommand command)
    {
        A.CallTo(() => _bankClient.ProcessPaymentAsync(A<BankRequest>.That.Matches(br =>
                br.PaymentId == command.Id
                && br.Amount == command.Amount
                && br.Card_Number == command.CardNumber
                && br.Currency == command.Currency
                && br.Cvv == command.Cvv
                && br.Expiry_Date == $"{command.ExpiryMonth}/{command.ExpiryYear}")))
            .MustHaveHappenedOnceExactly();
    }

    private void Then_the_payment_is_persisted(BankResponse bankResponse, CreatePaymentCommand createPaymentCommand)
    {
        A.CallTo(() =>
                _paymentsRepository.AddAsync(A<Payment>.That.Matches(p =>
                    p.Id == createPaymentCommand.Id
                    && p.Amount == createPaymentCommand.Amount
                    && p.Currency == createPaymentCommand.Currency
                    && p.ExpiryMonth == createPaymentCommand.ExpiryMonth
                    && p.ExpiryYear == createPaymentCommand.ExpiryYear
                    && p.CardNumberLastFour == "**** **** **** 4567"
                    && p.Status.ToString() == bankResponse.Status.ToString()
                    && p.Reason == bankResponse.Reason)))
            .MustHaveHappenedOnceExactly();
    }

    private void Then_the_response_contains_the_expected_information(CreatePaymentResponse response,
        CreatePaymentCommand command, BankResponse bankResponse)
    {
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Id, Is.EqualTo(command.Id));
            Assert.That(response.PaymentStatusCode.ToString(), Is.EqualTo(bankResponse.Status.ToString()));
            Assert.That(response.Amount, Is.EqualTo(command.Amount));
            Assert.That(response.Currency, Is.EqualTo(command.Currency));
            Assert.That(response.CardNumberLastFour, Is.EqualTo("**** **** **** 4567"));
            Assert.That(response.ExpiryMonth, Is.EqualTo(command.ExpiryMonth));
            Assert.That(response.ExpiryYear, Is.EqualTo(command.ExpiryYear));
            Assert.That(response.Reason, Is.EqualTo(bankResponse.Reason));
        });
        
        #endregion

    }

}