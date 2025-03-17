using FluentValidation;

using Microsoft.OpenApi.Models;

using PaymentGateway.Api.Api.Authentication;
using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.Commands.Validators;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
});
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentCommandValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter API Key",
        Name = Constants.ApiKeyHeaderName,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            []
        }
    });
});

builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddSingleton<IBankClient, BankClient>();

builder.Services.AddHttpClient<IBankClient, BankClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080/payments");
}).AddStandardResilienceHandler();

// Register MediatR
builder.Services.ConfigureApplication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ApiKeyMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
