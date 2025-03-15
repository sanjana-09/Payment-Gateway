using PaymentGateway.Api.Domain.BankClient;
using PaymentGateway.Api.Domain.Interfaces;

namespace PaymentGateway.Api.Infrastructure
{
    public class BankClient : IBankClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BankClient> _logger;

        public BankClient(HttpClient httpClient, ILogger<BankClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<BankResponse> ProcessPaymentAsync(BankRequest bankRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, bankRequest);

                _logger.LogInformation($"Bank response status code for paymentId {bankRequest.PaymentId}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                   var bankResponse = await response.Content.ReadFromJsonAsync<BankResponse>();

                   if(bankResponse is not null)
                       return bankResponse;
                }

                return new BankResponse(Authorized: false, Authorization_Code: null) { Reason = response.ReasonPhrase };
            }

            catch (Exception ex)
            {
                _logger.LogError($"Failed to communicate with acquiring bank with the error: {ex.Message}");
                throw ex;
            }

        }
    }
}
