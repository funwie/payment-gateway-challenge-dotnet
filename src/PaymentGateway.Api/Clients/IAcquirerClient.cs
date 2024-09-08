namespace PaymentGateway.Api.Clients;

public interface IAcquirerClient
{
    Task<AcquirerResponse> AcquireAsync(AcquirerRequest request, CancellationToken cancellationToken);
}