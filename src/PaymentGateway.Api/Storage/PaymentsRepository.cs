using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace PaymentGateway.Api.Storage;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly IDictionary<Guid, Payment> _payments = new ConcurrentDictionary<Guid, Payment>();

    public void Add(Payment payment)
    {
        _payments.Add(payment.Id, payment);
    }

    public Payment? Get(Guid id)
    {
        return _payments.TryGetValue(id, out Payment? payment) ? payment : null;
    }

    public IEnumerable<Payment> List()
    {
        return _payments.Values.ToImmutableList();
    }
}