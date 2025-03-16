namespace PaymentGateway.Api.Api.Authentication
{
    public interface IApiKeyValidation
    {
        bool IsValidApiKey(string? givenApiKey);
    }

    public class ApiKeyValidation : IApiKeyValidation
    {
        private readonly IConfiguration _configuration;

        public ApiKeyValidation(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsValidApiKey(string? givenApiKey)
        {
            if (string.IsNullOrWhiteSpace(givenApiKey))
                return false;

            string? apiKey = _configuration.GetValue<string>(Constants.ApiKeyName);

            return apiKey != null && apiKey == givenApiKey;
        }
    }
}
