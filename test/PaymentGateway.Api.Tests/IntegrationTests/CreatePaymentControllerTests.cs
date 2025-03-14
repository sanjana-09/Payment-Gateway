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
            var invalidCommand = new CreatePaymentCommand(Guid.Empty, "1234567890123456", 13, 
                2020, "XYZ", -100, "123");
            var content = new StringContent(JsonConvert.SerializeObject(invalidCommand), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/CreatePayment", content);

            // Assert

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));

            var rejectedPaymentResponse = await response.Content.ReadFromJsonAsync<RejectedPaymentResponse>();
            Assert.That(rejectedPaymentResponse, Is.Not.Null);
            Assert.That(rejectedPaymentResponse.Errors, Is.Not.Empty);
            Assert.That(rejectedPaymentResponse.Status, Is.EqualTo(PaymentStatus.Rejected.ToString()));
        }

        //[Test]
        //public async Task CreatePayment_Returns_InternalServerError_When_Processing_Fails()
        //{
        //    // Arrange
        //    var validCommand = new CreatePaymentCommand
        //    {
        //        // Provide valid data for testing success
        //    };

        //    var content = new StringContent(JsonConvert.SerializeObject(validCommand), Encoding.UTF8, "application/json");

        //    // Simulate a failure scenario (e.g., Mediator returning null or throwing exception)

        //    var response = await _client.PostAsync("/api/CreatePayment", content);

        //    // Assert
        //    response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);

        //    var responseBody = await response.Content.ReadAsStringAsync();
        //    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

        //    errorResponse.message.Should().Be("Internal server error");
        //    errorResponse.details.Should().Be("Something went wrong and the payment could not be processed. Please try again later.");
        //}

        //[Test]
        //public async Task CreatePayment_Returns_200_OK_When_Command_Is_Valid()
        //{
        //    // Arrange
        //    var validCommand = new CreatePaymentCommand
        //    {
        //        // Provide valid data for testing success
        //    };

        //    var content = new StringContent(JsonConvert.SerializeObject(validCommand), Encoding.UTF8, "application/json");

        //    // Act
        //    var response = await _client.PostAsync("/api/CreatePayment", content);

        //    // Assert
        //    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        //    var responseBody = await response.Content.ReadAsStringAsync();
        //    var paymentResponse = JsonConvert.DeserializeObject<CreatePaymentResponse>(responseBody);

        //    paymentResponse.Should().NotBeNull();
        //    paymentResponse.PaymentId.Should().NotBeEmpty();
        //}
    }
}

