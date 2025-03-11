using FluentValidation;

using PaymentGateway.Api.Application;
using PaymentGateway.Api.Application.Commands.Validators;
using PaymentGateway.Api.Domain.Interfaces;
using PaymentGateway.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register IHttpClientFactory
builder.Services.AddHttpClient();

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentCommandValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IBankSimulator, BankSimulator>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register MediatR
builder.Services.ConfigureApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
