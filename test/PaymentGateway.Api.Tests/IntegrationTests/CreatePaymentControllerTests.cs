using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Tests.IntegrationTests
{
    [TestFixture]
    public class CreatePaymentControllerTests
    {
        private WebApplicationFactory<CreatePaymentController> _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<CreatePaymentController>()
                .WithWebHostBuilder(builder =>
                    builder.ConfigureServices(services =>
                    {
                        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePaymentController).Assembly));
                        services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
                    }));

            _client = _factory.CreateClient();
        }

        [Test]
        public async Task Returns_400_BadRequest_when_command_is_invalid()
        {
            // Arrange
            var invalidCommand = new CreatePaymentCommand(Guid.Empty, 
                "1234567890123456", 
                13, 
                2020, 
                "XYZ", 
                -100,
                "123");

            var content = new StringContent(JsonConvert.SerializeObject(invalidCommand), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/CreatePayment", content);

            // Assert

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var rejectedPaymentResponse = await response.Content.ReadFromJsonAsync<RejectedPaymentResponse>();

            Assert.That(rejectedPaymentResponse, Is.Not.Null);
            Assert.That(rejectedPaymentResponse.Errors, Is.Not.Empty);
            Assert.That(rejectedPaymentResponse.Status, Is.EqualTo(PaymentStatus.Rejected.ToString()));
        }

        [Test]
        public async Task Returns_200_OK_with_expected_payment_details_when_command_is_valid()
        {
            var validCommand = new CreatePaymentCommand(
                Guid.NewGuid(),
                "1234567812345678",
                12,
                2025,
                "USD",
                100,
                "123"
            );

            var content = new StringContent(JsonConvert.SerializeObject(validCommand), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/CreatePayment", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var paymentResponse = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>();
            Assert.That(paymentResponse, Is.Not.Null);

            Then_the_response_contains_the_expected_payment_details(paymentResponse, validCommand);
        }

        #region Helper methods
        private void Then_the_response_contains_the_expected_payment_details(CreatePaymentResponse? paymentResponse, CreatePaymentCommand validCommand)
        {
            Assert.Multiple(() =>
            {
                Assert.That(paymentResponse.Id, Is.EqualTo(validCommand.Id));
                Assert.That(paymentResponse.CardNumberLastFour, Is.EqualTo("**** **** **** 5678"));
                Assert.That(paymentResponse.ExpiryMonth, Is.EqualTo(validCommand.ExpiryMonth));
                Assert.That(paymentResponse.ExpiryYear, Is.EqualTo(validCommand.ExpiryYear));
                Assert.That(paymentResponse.Currency, Is.EqualTo(validCommand.Currency));
                Assert.That(paymentResponse.Amount, Is.EqualTo(validCommand.Amount));
            });
        }


        #endregion
    }
}

