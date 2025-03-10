using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application.Queries;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

namespace PaymentGateway.Api.Tests.IntegrationTests;

[TestFixture]
public class GetPaymentsControllerTests
{
    private readonly Random _random = new();
    private IPaymentsRepository _paymentsRepository;
    private WebApplicationFactory<GetPaymentsController> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<GetPaymentsController>()
            .WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            }));
        _client = _factory.CreateClient();

        _paymentsRepository = _factory.Services.GetRequiredService<IPaymentsRepository>();
    }

    [Test]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new Payment()
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = $"**** **** **** **** {_random.Next(1111, 9999)}",
            Currency = "GBP"
        };

        _paymentsRepository.Add(payment);

        // Act
        var response = await _client.GetAsync($"/api/GetPayments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.That(HttpStatusCode.OK, Is.EqualTo(response.StatusCode));
        Assert.That(paymentResponse, Is.Not.Null);
    }

    [Test]
    public async Task Returns404IfPaymentNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/GetPayments/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}