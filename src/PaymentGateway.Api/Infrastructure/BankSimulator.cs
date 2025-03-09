using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public async Task<BankResponse?> ProcessPaymentAsync(BankPostPaymentRequest bankPostPaymentRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync("http://localhost:8080/payments", bankPostPaymentRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<BankResponse>(responseString);
            }

            return null;

        }
    }
}
