using FluentValidation;
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
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IBankClient, BankClient>();

builder.Services.AddHttpClient<IBankClient, BankClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080/payments");
}).AddStandardResilienceHandler();

// Register MediatR
builder.Services.ConfigureApplication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
