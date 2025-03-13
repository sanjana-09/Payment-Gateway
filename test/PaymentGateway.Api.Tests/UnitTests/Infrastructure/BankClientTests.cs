using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PaymentGateway.Api.Domain.BankClient;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Tests.UnitTests.Infrastructure
{
    public class BankClientTests
    {
        private IBankClient _bankClient;
        private ILogger<BankClient> _logger;
        private HttpClient _httpClient;
        private BankRequest _bankRequest;

        [SetUp]
        public void Setup()
        {
            _logger = A.Fake<ILogger<BankClient>>();
            _httpClient = new HttpClient();

            _bankClient = new BankClient(_httpClient, _logger);

            _bankRequest = new BankRequest(
                Card_Number: "1234567812345678",
                Expiry_Date: "12/2025",
                Currency: "USD",
                Amount: 1000,
                Cvv: "123"
            );
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task Returns_null_when_status_code_is_not_OK(HttpStatusCode statusCode)
        {
            var httpResponseMessage = new HttpResponseMessage(statusCode);
            A.CallTo(() => _httpClient.PostAsJsonAsync(A<string>._, _bankRequest, A<JsonSerializerOptions>._, A<CancellationToken>._)).Returns(httpResponseMessage);

            var result = await _bankClient.ProcessPaymentAsync(_bankRequest);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Returns_appropriate_bank_response_when_response_is_OK()
        {
            var bankResponse = new BankResponse(Authorized: true, Authorization_Code: "AUTH123");

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(bankResponse)
            };
            A.CallTo(() => _httpClient.PostAsJsonAsync(A<string>._, _bankRequest, A<JsonSerializerOptions>._, A<CancellationToken>._)).Returns(httpResponseMessage);

            var result = await _bankClient.ProcessPaymentAsync(_bankRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Authorized, Is.EqualTo(bankResponse.Authorized));
            Assert.That(result?.Authorization_Code, Is.EqualTo(bankResponse.Authorization_Code));
        }
    }
}
