using AutoFixture;

using FakeItEasy;

using NUnit.Framework;

using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Common;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;
using PaymentGateway.Api.Domain.Responses;

namespace PaymentGateway.Api.Tests.CommandHandler;

[TestFixture]
public class CreatePaymentRequestHandlerTests
{
    private IPaymentsRepository _paymentsRepository;
    private IBankSimulator _bankSimulator;
    private CreatePaymentRequestHandler _handler;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _bankSimulator = A.Fake<IBankSimulator>();
        _paymentsRepository = A.Fake<IPaymentsRepository>();

        _handler = new CreatePaymentRequestHandler(_bankSimulator, _paymentsRepository);

        _fixture = new Fixture();
    }

    [Test]
    public async Task Return_authorized_response_when_bank_authorizes_payment()
    {
        // Arrange
        var request = _fixture.Build<CreatePaymentRequest>()
            .With(x => x.CardNumber, "1234567891234567")
            .Create();

        var bankResponse = new BankResponse() { Authorized = true, Authorization_Code = "auth_code"};

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>._)).Returns(bankResponse);

        // Act
        var response = await _handler.Handle(request, default);

        // Assert

        A.CallTo(() => _bankSimulator.ProcessPaymentAsync(A<BankRequest>.That.Matches(br =>
            br.Amount == request.Amount 
            && br.Card_Number == request.CardNumber 
            && br.Currency == request.Currency
            && br.Cvv == request.Cvv 
            && br.Expiry_Date == $"{request.ExpiryMonth}/{request.ExpiryYear}")))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _paymentsRepository.Add(A<Payment>.That.Matches(p => p.Status == Payment.PaymentStatus.Authorized))).MustHaveHappenedOnceExactly();

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Status, Is.EqualTo(PaymentStatus.Authorized));
            Assert.That(response.Amount, Is.EqualTo(request.Amount));
            Assert.That(response.Currency, Is.EqualTo(request.Currency));
            Assert.That(response.CardNumberLastFour, Is.EqualTo("**** **** **** 4567"));
            Assert.That(response.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
            Assert.That(response.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        });
        
    }
}