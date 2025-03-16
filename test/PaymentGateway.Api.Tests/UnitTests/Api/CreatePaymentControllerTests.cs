using AutoFixture;

using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NUnit.Framework;
using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Responses;

namespace PaymentGateway.Api.Tests.UnitTests.Api
{
    [TestFixture]
    public class CreatePaymentControllerTests
    { 
        private IValidator<CreatePaymentCommand> _validator;
        private IMediator _mediator;
        private ILogger<CreatePaymentController> _logger;
        private Fixture _fixture;

        private CreatePaymentController _controller;

        [SetUp]
        public void Setup()
        {
            _validator = A.Fake<IValidator<CreatePaymentCommand>>();
            _mediator = A.Fake<IMediator>();
            _logger = A.Fake<ILogger<CreatePaymentController>>();

            _fixture = new Fixture();

            _controller = new CreatePaymentController(_validator, _mediator, _logger);
        }

        [Test]
        public async Task Returns_400_bad_request_when_payment_command_validation_fails()
        {
            // Arrange
            Given_command_validation_fails();

            var createPaymentCommand = _fixture.Create<CreatePaymentCommand>();

            // Act
            var result = await _controller.CreatePaymentAsync(createPaymentCommand);

            // Assert
            A.CallTo(() => _mediator.Send(A<CreatePaymentCommand>._, A<CancellationToken>._)).MustNotHaveHappened();

            Then_400_bad_request_with_rejected_payment_response_is_returned(result);
        }


        [TestCase(PaymentStatus.Declined, "Service Unavailable")]
        [TestCase(PaymentStatus.Authorized, "OK")]
        public async Task Returns_200_OK_when_payment_command_is_processed_successfully(PaymentStatus paymentStatus, string? reason)
        {
            // Arrange
            Given_command_validation_succeeds();

            var createPaymentCommand = _fixture.Create<CreatePaymentCommand>();
            var expectedPaymentResponse = new CreatePaymentResponse(Guid.NewGuid(), paymentStatus, 
                "**** **** **** 5678", 12, 2025, "USD", 1000, reason);

            A.CallTo(() => _mediator.Send(A<CreatePaymentCommand>._, A<CancellationToken>._)).Returns(expectedPaymentResponse);

            // Act
            var result = await _controller.CreatePaymentAsync(createPaymentCommand);

            //Assert
            A.CallTo(() => _mediator.Send(createPaymentCommand, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            Then_200_OK_with_with_expected_payment_information_is_returned(result, expectedPaymentResponse);
        }

        [Test]
        public async Task Returns_500_InternalServerError_when_response_is_null()
        {
            // Arrange
            Given_command_validation_succeeds();

            var createPaymentCommand = _fixture.Create<CreatePaymentCommand>();
            CreatePaymentResponse? nullResponse = null;

            A.CallTo(() => _mediator.Send(A<CreatePaymentCommand>._, A<CancellationToken>._))!.Returns(nullResponse);

            // Act
            var result = await _controller.CreatePaymentAsync(createPaymentCommand);

            // Assert
            A.CallTo(() => _mediator.Send(createPaymentCommand, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            Then_a_500_InternalServerError_is_returned(result);
        }

        #region Helper Methods
        private void Given_command_validation_fails()
        {
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Field", "Error") });
            A.CallTo(() => _validator.ValidateAsync(A<CreatePaymentCommand>._, A<CancellationToken>._)).Returns(validationResult);
        }
        private void Given_command_validation_succeeds()
        {
            var validationResult = A.Fake<ValidationResult>();
            A.CallTo(() => validationResult.IsValid).Returns(true);
            A.CallTo(() => _validator.ValidateAsync(A<CreatePaymentCommand>._, A<CancellationToken>._)).Returns(validationResult);
        }

        private void Then_400_bad_request_with_rejected_payment_response_is_returned(ActionResult result)
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.Value, Is.InstanceOf<RejectedPaymentResponse>());
            var rejectedPaymentResponse = badRequestResult.Value as RejectedPaymentResponse;

            Assert.That(rejectedPaymentResponse.Errors, Is.Not.Empty);
            Assert.That(rejectedPaymentResponse.Status, Is.EqualTo(PaymentStatus.Rejected.ToString()));
        }

        private void Then_200_OK_with_with_expected_payment_information_is_returned(ActionResult? result, CreatePaymentResponse expectedPaymentResponse)
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.Multiple(() =>
            {
                var returnedPaymentResponse = okResult.Value as CreatePaymentResponse;
                Assert.That(returnedPaymentResponse, Is.Not.Null);
                Assert.That(returnedPaymentResponse.Id, Is.EqualTo(expectedPaymentResponse.Id));
                Assert.That(returnedPaymentResponse.Status, Is.EqualTo(expectedPaymentResponse.Status));
                Assert.That(returnedPaymentResponse.CardNumberLastFour, Is.EqualTo(expectedPaymentResponse.CardNumberLastFour));
                Assert.That(returnedPaymentResponse.ExpiryMonth, Is.EqualTo(expectedPaymentResponse.ExpiryMonth));
                Assert.That(returnedPaymentResponse.ExpiryYear, Is.EqualTo(expectedPaymentResponse.ExpiryYear));
                Assert.That(returnedPaymentResponse.Currency, Is.EqualTo(expectedPaymentResponse.Currency));
                Assert.That(returnedPaymentResponse.Amount, Is.EqualTo(expectedPaymentResponse.Amount));
                Assert.That(returnedPaymentResponse.Reason, Is.EqualTo(expectedPaymentResponse.Reason));
            });
        }

        private void Then_a_500_InternalServerError_is_returned(ActionResult result)
        {
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var actionResult = result as ObjectResult;
            Assert.That(actionResult.StatusCode, Is.EqualTo(500));
        }

        #endregion
    }
}
