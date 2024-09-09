using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<(PaymentResponse?, ErrorResponse?)> AuthorizeAsync(PaymentRequest paymentRequest,
        CancellationToken cancellationToken);

    Task<(PaymentResponse?, ErrorResponse?)> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IEnumerable<PaymentResponse>, ErrorResponse?)> ListAsync(CancellationToken cancellationToken);
}