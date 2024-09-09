namespace PaymentGateway.Api;

public record Settings
{
    public Acquirer Acquirer { get; init; }
    public HashSet<string> SupportedCurrencies { get; init; } = new();
}

public record Acquirer
{
    public Uri BaseUrl { get; init; }
}