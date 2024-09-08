namespace PaymentGateway.Api.Storage;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
    IEnumerable<Payment> List();
}