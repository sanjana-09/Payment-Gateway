using AutoFixture;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NUnit.Framework;
using PaymentGateway.Api.Api.Controllers;
using PaymentGateway.Api.Application.Queries;

namespace PaymentGateway.Api.Tests.UnitTests.Api;

[TestFixture]
public class GetPaymentControllerTests
{
    private IMediator _mediator;
    private ILogger<GetPaymentController> _logger;
    private Fixture _fixture;
    private GetPaymentController _controller;

    [SetUp]
    public void Setup()
    {
        _mediator = A.Fake<IMediator>();
        _logger = A.Fake<ILogger<GetPaymentController>>();
        _fixture = new Fixture();
        _controller = new GetPaymentController(_mediator, _logger);
    }

    [Test]
    public async Task Return_200_OK_with_payment_details_when_it_exists()
    {
        //Arrange
        var paymentId = Guid.NewGuid();
        var expectedResponse = _fixture.Create<GetPaymentResponse>();

        A.CallTo(() => _mediator.Send(A<GetPaymentQuery>._, A<CancellationToken>._)).Returns(expectedResponse);

        //Act
        var result = await _controller.GetPaymentAsync(paymentId);

        //Assert
        A.CallTo(() => _mediator.Send(A<GetPaymentQuery>.That.Matches(q => q.Id == paymentId), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task Returns_404_when_payment_does_not_exist()
    {
        //Arrange
        GetPaymentResponse? response = null;

        var paymentId = Guid.NewGuid();

        A.CallTo(() => _mediator.Send(A<GetPaymentQuery>.Ignored, A<CancellationToken>._))!.Returns(response);

        //Act
        var result = await _controller.GetPaymentAsync(paymentId);

        //Assert
        A.CallTo(() => _mediator.Send(A<GetPaymentQuery>.That.Matches(q => q.Id == paymentId), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
