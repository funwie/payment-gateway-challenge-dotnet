namespace PaymentGateway.Api.Clients;

public record AcquirerRequest
{
    public string CardNumber { get; set; }
    public string ExpiryDate { get; set; }
    public string Currency { get; set; }
    public long Amount { get; set; }
    public string Cvv { get; set; }
}