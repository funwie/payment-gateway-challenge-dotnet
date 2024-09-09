namespace PaymentGateway.Api.Clients;

public record AcquirerResponse(bool Authorized, string AuthorizationCode);