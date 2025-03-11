using AutoFixture;
using FluentValidation.TestHelper;
using NUnit.Framework;
using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Application.Commands.Validators;

namespace PaymentGateway.Api.Tests.UnitTests.Application;

[TestFixture]
public class CreatePaymentCommandValidatorTests
{
    private CreatePaymentCommandValidator _validator;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _validator = new CreatePaymentCommandValidator();
        _fixture = new Fixture();
    }

    #region Card Number

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Should_Have_Error_When_CardNumber_Is_Empty(string invalidCardNumber)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.CardNumber, invalidCardNumber).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number is required");
    }

    [TestCase("123456789012345678901")]
    [TestCase("123456")]
    public void Should_Have_Error_When_CardNumber_Length_Is_Invalid(string invalidCardNumber)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.CardNumber, invalidCardNumber).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number must be between 14 and 19 characters");
    }

    [Test]
    public void Should_Have_Error_When_CardNumber_Contains_NonNumeric_Characters()
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.CardNumber, "1234A567890").Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number must only contain numeric characters");
    }

    [Test]
    public void Should_Not_Have_Error_When_CardNumber_Is_Valid()
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.CardNumber, "1234567890123456").Create();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
    }

    #endregion

    #region Expiry Date

    [TestCase(0)]
    [TestCase(13)]
    public void Should_Have_Error_When_ExpiryMonth_Is_Invalid(int invalidExpiryMonth)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.ExpiryMonth, invalidExpiryMonth).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month must be between 1 and 12");
    }

    [Test]
    public void Should_Have_Error_When_ExpiryYear_Is_In_Past()
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.ExpiryYear, DateTime.Now.Year - 1).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage("Expiry year must be in the future");
    }

    [Test]
    public void Should_Have_Error_When_ExpiryMonth_Is_In_The_Past_And_ExpiryYear_Is_Current()
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.ExpiryMonth, DateTime.Now.Month - 1)
            .With(x => x.ExpiryYear, DateTime.Now.Year).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage("Expiry month and year combination must be in the future");
    }

    [Test]
    public void Should_Pass_When_ExpiryMonth_Is_Current_Or_Future_And_ExpiryYear_Is_Current()
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.ExpiryMonth, DateTime.Now.Month)
            .With(x => x.ExpiryYear, DateTime.Now.Year).Create();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryMonth);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryYear);
    }

    #endregion

    #region Currency
    [TestCase("XYZ")]
    [TestCase("")]
    public void Should_Have_Error_When_Currency_Is_Invalid(string invalidCurrency)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.Currency, invalidCurrency).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be one of the valid ISO codes");
    }

    [TestCase("GBP")]
    [TestCase("USD")]
    [TestCase("EUR")]
    public void Should_Pass_When_Currency_Is_Valid(string currency)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.Currency, currency).Create();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }


    #endregion

    #region Amount

    [TestCase(-10)]
    [TestCase(0)]
    public void Should_Have_Error_When_Amount_Is_Invalid(int invalidAmount)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.Amount, invalidAmount).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be a positive integer");
    }

    #endregion

    #region Cvv

    [TestCase("12347")]
    [TestCase("99")]
    [TestCase("")]
    public void Should_Have_Error_When_CVV_Is_Invalid(string invalidCvv)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.Cvv, invalidCvv).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Cvv)
            .WithErrorMessage("CVV must be 3 to 4 characters long");
    }

    [TestCase("abc")]
    [TestCase("12c-")]
    public void Should_Have_Error_When_CVV_Is_Invalid_With_Non_Numeric_Chars(string invalidCvv)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.Cvv, invalidCvv).Create();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Cvv)
            .WithErrorMessage("CVV must only contain numeric characters");
    }

    [TestCase("123")]
    [TestCase("1234")]
    public void Should_Pass_When_CVV_Is_Valid(string Cvv)
    {
        var model = _fixture.Build<CreatePaymentCommand>().With(x => x.Cvv, Cvv).Create();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Cvv);
    }

    #endregion
}
