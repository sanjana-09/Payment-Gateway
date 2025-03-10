namespace PaymentGateway.Api.Domain.Requests;

public class BankRequest
{
    public string Card_Number { get; set; }
    public string Expiry_Date { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Cvv { get; set; }
}