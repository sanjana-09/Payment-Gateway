﻿using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

using PaymentGateway.Api.Api.Authentication;
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
        private string _apiKey;

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

            var configuration = _factory.Services.GetRequiredService<IConfiguration>();
            _apiKey = configuration["ApiKey"];

            _client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, _apiKey);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("wrong key")]
        public async Task Returns_401_Unauthorized_when_api_key_is_missing_or_invalid(string invalidApiKey)
        {
            var validCommand = new CreatePaymentCommand(
                Guid.NewGuid(),
                "123456781234567",
                12,
                2025,
                "USD",
                100,
                "123"
            );

            var content = new StringContent(JsonConvert.SerializeObject(validCommand), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Remove(Constants.ApiKeyHeaderName);
            _client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, invalidApiKey);

            var response = await _client.PostAsync("/api/CreatePayment", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
            Assert.That(rejectedPaymentResponse!.Errors, Is.Not.Empty);
            Assert.That(rejectedPaymentResponse.PaymentStatus, Is.EqualTo(PaymentStatus.Rejected.ToString()));
        }

        [TestCase("123456781234567", "4567",PaymentStatus.Authorized)]
        [TestCase("123456781234568", "4568", PaymentStatus.Declined)]
        public async Task Returns_200_OK_with_expected_payment_details_when_command_is_valid(string cardNumber, string lastFour, PaymentStatus paymentStatus)
        {
            //Arrange
            var validCommand = new CreatePaymentCommand(
                Guid.NewGuid(),
                cardNumber,
                12,
                2025,
                "USD",
                100,
                "123"
            );

            var content = new StringContent(JsonConvert.SerializeObject(validCommand), Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PostAsync("/api/CreatePayment", content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var paymentResponse = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>();
            Assert.That(paymentResponse, Is.Not.Null);

            Then_the_response_contains_the_expected_payment_details(paymentResponse, validCommand, paymentStatus, lastFour);
        }


        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        #region Helper methods
        private void Then_the_response_contains_the_expected_payment_details(CreatePaymentResponse paymentResponse, CreatePaymentCommand validCommand, PaymentStatus paymentStatus, string lastFour)
        {
            Assert.Multiple(() =>
            {
                Assert.That(paymentResponse!.Id, Is.EqualTo(validCommand.Id));
                Assert.That(paymentResponse.CardNumberLastFour, Is.EqualTo($"**** **** **** {lastFour}"));
                Assert.That(paymentResponse.ExpiryMonth, Is.EqualTo(validCommand.ExpiryMonth));
                Assert.That(paymentResponse.ExpiryYear, Is.EqualTo(validCommand.ExpiryYear));
                Assert.That(paymentResponse.Currency, Is.EqualTo(validCommand.Currency));
                Assert.That(paymentResponse.Amount, Is.EqualTo(validCommand.Amount));
                Assert.That(paymentResponse.PaymentStatusCode, Is.EqualTo(paymentStatus));
                Assert.That(paymentResponse.PaymentStatus, Is.EqualTo(paymentStatus.ToString()));
            });
        }
        #endregion
    }
}

