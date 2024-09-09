namespace PaymentGateway.Api.Models.Responses;

public class ErrorResponse
{
    public ErrorResponse(string errorType, IEnumerable<string>? errorMessages = null)
    {
        if (string.IsNullOrEmpty(errorType))
        {
            throw new ArgumentNullException(nameof(errorType));
        }

        ErrorType = errorType;
        ErrorMessages = errorMessages ?? Enumerable.Empty<string>();
    }

    public string ErrorType { get; }
    public IEnumerable<string> ErrorMessages { get; }
}