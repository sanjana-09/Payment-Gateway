using NUnit.Framework;
using FluentValidation.TestHelper;

using PaymentGateway.Api.Api.Validators;
using PaymentGateway.Api.Application.Commands;

namespace PaymentGateway.Api.Tests;

[TestFixture]
public class CreatePaymentRequestValidatorTests
{
    private CreatePaymentRequestValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreatePaymentRequestValidator();
    }

    #region Card Number

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Should_Have_Error_When_CardNumber_Is_Empty(string invalidCardNumber)
    {
        var model = new CreatePaymentRequest { CardNumber = invalidCardNumber };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number is required");
    }

    [TestCase("123456789012345678901")]
    [TestCase("123456")]
    public void Should_Have_Error_When_CardNumber_Length_Is_Invalid(string cardNumber)
    {
        var model = new CreatePaymentRequest { CardNumber = cardNumber };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number must be between 14 and 19 characters");
    }

    [Test]
    public void Should_Have_Error_When_CardNumber_Contains_NonNumeric_Characters()
    {
        var model = new CreatePaymentRequest { CardNumber = "1234A567890" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number must only contain numeric characters");
    }

    [Test]
    public void Should_Not_Have_Error_When_CardNumber_Is_Valid()
    {
        var model = new CreatePaymentRequest { CardNumber = "1234567890123456" };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
    }

    #endregion

    #region Expiry Date

    [TestCase(0)]
    [TestCase(13)]
    public void Should_Have_Error_When_ExpiryMonth_Is_Invalid(int invalidExpiryMonth)
    {
        var model = new CreatePaymentRequest { ExpiryMonth = invalidExpiryMonth };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month must be between 1 and 12");
    }

    [Test]
    public void Should_Have_Error_When_ExpiryYear_Is_In_Past()
    {
        var model = new CreatePaymentRequest { ExpiryMonth = 12, ExpiryYear = DateTime.Now.Year - 1 };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage("Expiry year must be in the future");
    }

    [Test]
    public void Should_Have_Error_When_ExpiryMonth_Is_In_The_Past_And_ExpiryYear_Is_Current()
    {
        var model = new CreatePaymentRequest { ExpiryMonth = DateTime.Now.Month - 1, ExpiryYear = DateTime.Now.Year };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage("Expiry month and year combination must be in the future");
    }

    [Test]
    public void Should_Pass_When_ExpiryMonth_Is_Current_Or_Future_And_ExpiryYear_Is_Current()
    {
        var model = new CreatePaymentRequest { ExpiryMonth = DateTime.Now.Month, ExpiryYear = DateTime.Now.Year };

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
        var model = new CreatePaymentRequest { Currency = invalidCurrency };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be one of the valid ISO codes");
    }

    [TestCase("GBP")]
    [TestCase("USD")]
    [TestCase("EUR")]
    public void Should_Pass_When_Currency_Is_Valid(string currency)
    {
        var model = new CreatePaymentRequest { Currency = currency };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }


    #endregion

    #region Amount

    [TestCase(-10)]
    [TestCase(0)]
    public void Should_Have_Error_When_Amount_Is_Invalid(int invalidAmount)
    {
        var model = new CreatePaymentRequest { Amount = invalidAmount };

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
        var model = new CreatePaymentRequest { Cvv = invalidCvv };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Cvv)
            .WithErrorMessage("CVV must be 3 to 4 characters long");
    }

    [TestCase("123")]
    [TestCase("1234")]
    public void Should_Pass_When_CVV_Is_Valid(string Cvv)
    {
        var model = new CreatePaymentRequest { Cvv = Cvv };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Cvv);
    }

    #endregion
}
