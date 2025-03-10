using System.Net;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Domain.Requests;
using PaymentGateway.Api.Domain.Responses;

namespace PaymentGateway.Api.Infrastructure
{
    public class BankSimulator : IBankSimulator
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BankSimulator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BankResponse?> ProcessPaymentAsync(BankRequest bankRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();
            string? bankUrl = "http://localhost:8080/payments";

            var response = await httpClient.PostAsJsonAsync(bankUrl, bankRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<BankResponse>();
            }

            return null;

        }
    }
}
