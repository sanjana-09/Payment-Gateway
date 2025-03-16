using System.Net;

namespace PaymentGateway.Api.Api.Authentication
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiKeyValidation _apiKeyValidation;

        public ApiKeyMiddleware(RequestDelegate next, IApiKeyValidation apiKeyValidation)
        {
            _next = next;
            _apiKeyValidation = apiKeyValidation;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? userApiKey = context.Request.Headers[Constants.ApiKeyHeaderName];

            if (!_apiKeyValidation.IsValidApiKey(userApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                var response = new { message = "Invalid or missing API key" };
                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }
    }
}
