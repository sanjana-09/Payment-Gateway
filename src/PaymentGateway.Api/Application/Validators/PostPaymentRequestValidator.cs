using FluentValidation;

using PaymentGateway.Api.Application.DTOs.Requests;

namespace PaymentGateway.Api.Application.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
       public PostPaymentRequestValidator() 
       { 
            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Card number is required")
                .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters")
                .Matches(@"^\d+$").WithMessage("Card number must only contain numeric characters");

            RuleFor(x => x.ExpiryMonth)
                .NotEmpty().WithMessage("Expiry month is required")
                .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty().WithMessage("Expiry year is required")
                .GreaterThanOrEqualTo(DateTime.Now.Year).WithMessage("Expiry year must be in the future")
                .Must((model, year) => model.ExpiryMonth >= DateTime.Now.Month || year > DateTime.Now.Year)
                .WithMessage("Expiry month and year combination must be in the future");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency code must be exactly 3 characters")
                .Must(BeValidCurrency).WithMessage("Currency must be one of the valid ISO codes");

            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Amount is required")
                .GreaterThan(0).WithMessage("Amount must be a positive integer");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("CVV is required")
                .Length(3, 4).WithMessage("CVV must be 3 to 4 characters long")
                .Matches(@"^\d+$").WithMessage("CVV must only contain numeric characters");
       }

        private bool BeValidCurrency(string currency)
        {
            var validCurrencies = new List<string>
            {
                "USD", "EUR", "GBP",
            };

            return validCurrencies.Contains(currency);
        }
}