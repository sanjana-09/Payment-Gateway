using FluentValidation;
using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.Commands.Validators;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

using Polly;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentCommandValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IBankClient, BankClient>();

builder.Services.AddHttpClient<BankClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:8080/payments");
    })
    .AddResilienceHandler("Timeout", resilienceBuilder =>
    { 
        resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(5));
    });

// Register MediatR
builder.Services.ConfigureApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
