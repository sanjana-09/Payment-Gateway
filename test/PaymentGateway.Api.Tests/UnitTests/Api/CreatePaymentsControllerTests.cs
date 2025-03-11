using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;
using PaymentGateway.Api.Application.Common;

namespace PaymentGateway.Api.Tests.UnitTests.Api
{
    [TestFixture]
    public class CreatePaymentsControllerTests
    { 
        private IValidator<CreatePaymentCommand> _validator;
        private IMediator _mediator;

        private CreatePaymentsController _controller;

        [SetUp]
        public void Setup()
        {
            _validator = A.Fake<IValidator<CreatePaymentCommand>>();
            _mediator = A.Fake<IMediator>();

            _controller = new CreatePaymentsController(_validator, _mediator);
        }

        [Test]
        public async Task Returns_400_bad_request_when_request_validation_fails()
        {
            // Arrange
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Field", "Error") });

            A.CallTo(() => _validator.ValidateAsync(A<CreatePaymentCommand>._, A<CancellationToken>._)).Returns(Task.FromResult(validationResult));

            var createPaymentCommand = new CreatePaymentCommand(
                CardNumber: "1234567812345678",
                ExpiryMonth: 12,
                ExpiryYear: 2025,
                Currency: "USD",
                Amount: 1000,
                Cvv: "123"
            );
            // Act
            var result = await _controller.CreatePaymentAsync(createPaymentCommand);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.Value, Is.InstanceOf<RejectedPaymentResponse>());

        }

        [Test]
        public async Task Returns_200_OK_when_payment_request_is_processed_successfully()
        {
            // Arrange
            var createPaymentCommand = new CreatePaymentCommand(
                CardNumber: "1234567812345678",
                ExpiryMonth: 12,
                ExpiryYear: 2025,
                Currency: "USD",
                Amount: 1000,
                Cvv: "123"
            );

            var validationResult = A.Fake<ValidationResult>();
            A.CallTo(() => validationResult.IsValid).Returns(true);
            A.CallTo(() => _validator.ValidateAsync(A<CreatePaymentCommand>._, A<CancellationToken>._)).Returns(Task.FromResult(validationResult));

            var paymentResponse = new CreatePaymentResponse(Guid.NewGuid(), PaymentStatus.Authorized, 
                "**** **** **** 3456", 12, 2089, "GBP", 100);

            A.CallTo(() => _mediator.Send(createPaymentCommand, A<CancellationToken>._)).Returns(paymentResponse);

            // Act
            var result = await _controller.CreatePaymentAsync(createPaymentCommand);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(paymentResponse));
        }
    }
}
