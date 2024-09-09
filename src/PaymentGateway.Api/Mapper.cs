using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Storage;

namespace PaymentGateway.Api;

public static class Mapper
{
    public static (PaymentResponse?, ErrorResponse?) ToPaymentResponse(Payment payment)
    {
        if (Enum.TryParse(payment.Status, out PaymentStatus paymentStatus) is false)
        {
            ErrorResponse error = new(ErrorTypes.InternalError, new[] { Errors.DataParsingError });
            return (null, error);
        }

        PaymentResponse paymentResponse = new()
        {
            Id = payment.Id,
            Status = paymentStatus,
            CardNumberLastFour = payment.CardNumber[^4..],
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };

        return (paymentResponse, null);
    }

    public static Payment ToPayment(PaymentRequest paymentRequest, AcquirerResponse acquirerResponse)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Status = (acquirerResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined).ToString(),
            CardNumber = paymentRequest.CardNumber,
            ExpiryMonth = paymentRequest.ExpiryMonth!.Value,
            ExpiryYear = paymentRequest.ExpiryYear!.Value,
            Cvv = paymentRequest.Cvv,
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount!.Value,
            AuthorizationResult =
                new AuthorizationResult(acquirerResponse.Authorized, acquirerResponse.AuthorizationCode),
            CreatedAt = DateTime.UtcNow
        };
    }
}