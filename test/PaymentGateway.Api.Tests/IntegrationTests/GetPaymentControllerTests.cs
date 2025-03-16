using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

using PaymentGateway.Api.Api.Authentication;
using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application.Queries;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Tests.IntegrationTests;

[TestFixture]
public class GetPaymentControllerTests
{
    private readonly Random _random = new();
    private IPaymentsRepository _paymentsRepository;
    private WebApplicationFactory<GetPaymentController> _factory;
    private HttpClient _client;
    private string? _apiKey;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<GetPaymentController>()
            .WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPaymentController).Assembly));
                services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            }));
        _client = _factory.CreateClient();

        _paymentsRepository = _factory.Services.GetRequiredService<IPaymentsRepository>();
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        _apiKey = configuration["ApiKey"];

        _client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, _apiKey);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("wrong key")]
    public async Task Returns_401_Unauthorized_when_api_key_is_missing_or_invalid(string? invalidApiKey)
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove(Constants.ApiKeyHeaderName);
        _client.DefaultRequestHeaders.Add(Constants.ApiKeyHeaderName, invalidApiKey);

        // Act
        var response = await _client.GetAsync($"/api/GetPayment/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Retrieves_a_payment_successfully()
    {
        // Arrange
        var payment = new Payment()
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = $"**** **** **** **** {_random.Next(1111, 9999)}",
            Currency = "GBP",
            Status = Payment.PaymentStatus.Declined,
            Reason = "Insufficient funds"
        };

        await _paymentsRepository.AddAsync(payment);

        // Act
        var response = await _client.GetAsync($"/api/GetPayment/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(paymentResponse, Is.Not.Null);
        Then_the_response_contains_the_expected_details(paymentResponse, payment);
    }

    [Test]
    public async Task Returns_404_if_payment_not_found()
    {
        // Act
        var response = await _client.GetAsync($"/api/GetPayment/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    #region Helper methods
    private void Then_the_response_contains_the_expected_details(
        GetPaymentResponse? paymentResponse, Payment payment)
    {
        Assert.Multiple(() =>
        {
            Assert.That(paymentResponse.Id, Is.EqualTo(payment.Id));
            Assert.That(paymentResponse.Amount, Is.EqualTo(payment.Amount));
            Assert.That(paymentResponse.Currency, Is.EqualTo(payment.Currency));
            Assert.That(paymentResponse.ExpiryMonth, Is.EqualTo(payment.ExpiryMonth));
            Assert.That(paymentResponse.ExpiryYear, Is.EqualTo(payment.ExpiryYear));
            Assert.That(paymentResponse.CardNumberLastFour, Is.EqualTo(payment.CardNumberLastFour));
            Assert.That(paymentResponse.PaymentStatusCode.ToString(), Is.EqualTo(payment.Status.ToString()));
            Assert.That(paymentResponse.Status, Is.EqualTo(payment.Status.ToString()));
            Assert.That(paymentResponse.Reason, Is.EqualTo(payment.Reason));
        });
    }
    #endregion
}