using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Storage;

namespace PaymentGateway.Api.Services;

public class PaymentService(
    IAcquirerClient acquirerClient,
    IPaymentsRepository paymentRepository,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<(PaymentResponse?, ErrorResponse?)> AuthorizeAsync(PaymentRequest paymentRequest,
        CancellationToken cancellationToken)
    {
        AcquirerRequest acquirerRequest = new()
        {
            CardNumber = paymentRequest.CardNumber,
            ExpiryDate = $"{paymentRequest.ExpiryMonth:D2}/{paymentRequest.ExpiryYear}",
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount!.Value,
            Cvv = paymentRequest.Cvv
        };

        try
        {
            AcquirerResponse acquirerResponse = await acquirerClient.AcquireAsync(acquirerRequest, cancellationToken);
            Payment payment = Mapper.ToPayment(paymentRequest, acquirerResponse);
            paymentRepository.Add(payment);
            return Mapper.ToPaymentResponse(payment);
        }
        catch (AcquirerException ex)
        {
            logger.LogError(ex, "Acquiring request error");
            ErrorResponse error = new(ErrorTypes.InternalError, new[] { Errors.AcquiringError });
            return (null, error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Payment processing error");
            ErrorResponse error = new(ErrorTypes.InternalError, new[] { Errors.ProcessingError });
            return (null, error);
        }
    }

    public Task<(PaymentResponse?, ErrorResponse?)> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Payment? payment = paymentRepository.Get(id);
        return payment is null
            ? Task.FromResult<(PaymentResponse?, ErrorResponse?)>((null, null))
            : Task.FromResult(Mapper.ToPaymentResponse(payment));
    }

    public Task<(IEnumerable<PaymentResponse>, ErrorResponse?)> ListAsync(CancellationToken cancellationToken)
    {
        IEnumerable<PaymentResponse?> paymentResponses = paymentRepository.List().Select(p =>
        {
            (PaymentResponse? paymentResponse, _) = Mapper.ToPaymentResponse(p);
            return paymentResponse;
        });
        return Task.FromResult<(IEnumerable<PaymentResponse>, ErrorResponse?)>((paymentResponses, null));
    }
}