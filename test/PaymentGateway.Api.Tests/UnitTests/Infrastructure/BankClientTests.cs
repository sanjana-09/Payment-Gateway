using System.Net;
using System.Net.Http.Json;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PaymentGateway.Api.Domain.BankClient;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Tests.UnitTests.Infrastructure
{
    public class BankClientTests
    {
        private ILogger<BankClient> _logger;
        private BankRequest _bankRequest;

        [SetUp]
        public void Setup()
        {
            _logger = A.Fake<ILogger<BankClient>>();

            _bankRequest = new BankRequest(
                PaymentId: Guid.NewGuid(), 
                Card_Number: "1234567812345678",
                Expiry_Date: "12/2025",
                Currency: "USD",
                Amount: 1000,
                Cvv: "123"
            );
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task Returns_declined_response_when_status_code_is_not_OK(HttpStatusCode statusCode)
        {
            //Arrange
            using var httpResponseMessage = new HttpResponseMessage(statusCode);

            using HttpClient httpClient = Given_the_httpClient_returns_this_response(httpResponseMessage);

            //Act
            var bankClient = new BankClient(httpClient, _logger);

            var result = await bankClient.ProcessPaymentAsync(_bankRequest);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Authorized, Is.False);
                Assert.That(result.Status, Is.EqualTo(BankResponseStatus.Declined));
            });

        }

        [TestCase(true, "AUTH123", BankResponseStatus.Authorized)]
        [TestCase(false,null, BankResponseStatus.Declined)]
        public async Task Returns_appropriate_bank_response_when_response_is_OK(bool authorized, string authCode, BankResponseStatus bankResponseStatus)
        {
            //Arrange
            var expectedBankResponse = new BankResponse(authorized, authCode);

            using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(expectedBankResponse);

            using var httpClient = Given_the_httpClient_returns_this_response(httpResponseMessage);

            //Act
            var bankClient = new BankClient(httpClient, _logger);

            var result = await bankClient.ProcessPaymentAsync(_bankRequest);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Authorized, Is.EqualTo(expectedBankResponse.Authorized));
                Assert.That(result.Status, Is.EqualTo(expectedBankResponse.Status));
            });
        }

        #region Helper methods

        private HttpClient Given_the_httpClient_returns_this_response(HttpResponseMessage httpResponseMessage)
        {
            var handler = A.Fake<FakeableHttpMessageHandler>();
            A.CallTo(() => handler.FakeSendAsync(
                    A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
                .Returns(httpResponseMessage);

            HttpClient httpClient = new(handler);
            httpClient.BaseAddress = new Uri("http://mockedbaseuri");
            return httpClient;
        
        }

        #endregion
    }
    public abstract class FakeableHttpMessageHandler : HttpMessageHandler
    {
        public abstract Task<HttpResponseMessage> FakeSendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken);

        // sealed so FakeItEasy won't intercept calls to this method
        protected sealed override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => this.FakeSendAsync(request, cancellationToken);
    }
}
