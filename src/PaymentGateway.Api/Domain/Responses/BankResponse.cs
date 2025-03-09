namespace PaymentGateway.Api.Domain.Responses;

public class BankResponse
{
    public bool Authorized { get; set; }
    public string Authorization_Code { get; set; }
}