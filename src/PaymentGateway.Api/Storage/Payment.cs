namespace PaymentGateway.Api.Storage;

public record Payment
{
    public Guid Id { get; init; }
    public string Status { get; init; }
    public string CardNumber { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; }
    public long Amount { get; init; }
    public string Cvv { get; init; }
    public AuthorizationResult? AuthorizationResult { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AuthorizationResult(bool Authorized, string AuthorizationCode);