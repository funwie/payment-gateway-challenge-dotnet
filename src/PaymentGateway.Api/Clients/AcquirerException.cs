namespace PaymentGateway.Api.Clients;

public class AcquirerException : Exception
{
    public AcquirerException(string message) : base(message) { }
    public AcquirerException(string message, Exception inner) : base(message, inner) { }
}