using FluentValidation;

using PaymentGateway.Api.Application.DTOs.Requests;
using PaymentGateway.Api.Application.DTOs.Responses;

namespace PaymentGateway.Api.Application
{
    public interface IPaymentService
    {
        Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest postPaymentRequest);
    }
    public class PaymentService: IPaymentService
    {
        private readonly IValidator<PostPaymentRequest> _validator;

        public PaymentService(IValidator<PostPaymentRequest> validator)
        {
            _validator = validator;
        }

        public Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest postPaymentRequest)
        {
            throw new NotImplementedException();
        }
    }
}
