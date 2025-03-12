using System.Net;

using PaymentGateway.Api.Domain.BankClient;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Infrastructure
{
    public class BankClient : IBankClient
    {
        private readonly HttpClient _httpClient;

        public BankClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BankResponse?> ProcessPaymentAsync(BankRequest bankRequest)
        {
            var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, bankRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<BankResponse>();
            }

            return null;

        }
    }
}
