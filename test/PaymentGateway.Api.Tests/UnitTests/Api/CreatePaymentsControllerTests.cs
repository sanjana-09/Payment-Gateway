using FakeItEasy;

using FluentValidation;
using FluentValidation.Results;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.Commands;

namespace PaymentGateway.Api.Tests.UnitTests.Api
{
    [TestFixture]
    public class CreatePaymentsControllerTests
    { 
        private IValidator<CreatePaymentRequest> _validator;
        private IMediator _mediator;

        private CreatePaymentsController _controller;

        [SetUp]
        public void Setup()
        {
            _validator = A.Fake<IValidator<CreatePaymentRequest>>();
            _mediator = A.Fake<IMediator>();

            _controller = new CreatePaymentsController(_validator, _mediator);
        }

        [Test]
        public async Task Returns_400_bad_request_when_request_validation_fails()
        {
            // Arrange
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Field", "Error") });

            A.CallTo(() => _validator.ValidateAsync(A<CreatePaymentRequest>._, A<CancellationToken>._)).Returns(Task.FromResult(validationResult));

            // Act
            var result = await _controller.CreatePaymentAsync(new CreatePaymentRequest());

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Returns_200_OK_when_payment_request_is_processed_successfully()
        {
            // Arrange
            var request = new CreatePaymentRequest();

            var validationResult = A.Fake<ValidationResult>();
            A.CallTo(() => validationResult.IsValid).Returns(true);
            A.CallTo(() => _validator.ValidateAsync(A<CreatePaymentRequest>._, A<CancellationToken>._)).Returns(Task.FromResult(validationResult));

            var paymentResponse = new PostPaymentResponse();
            A.CallTo(() => _mediator.Send(request, A<CancellationToken>._)).Returns(paymentResponse);

            // Act
            var result = await _controller.CreatePaymentAsync(request);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(paymentResponse));
        }
    }
}
