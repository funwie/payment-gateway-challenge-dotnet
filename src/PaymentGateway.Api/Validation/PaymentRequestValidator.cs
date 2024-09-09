using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Validation;

public interface IPaymentRequestValidator
{
    ValidationResult Validate(PaymentRequest? payment);
}

public class PaymentRequestValidator(Settings settings) : IPaymentRequestValidator
{
    private const int MinCvvLength = 3;
    private const int MaxCvvLength = 4;
    private const int MinCardNumberLength = 14;
    private const int MaxCardNumberLength = 19;
    private const int MinExpiryMonth = 1;
    private const int MaxExpiryMonth = 12;
    private const int MinAmount = 1;
    private const int MaxYear = 9999;
    private const int CurrencyCharactersLength = 3;

    private readonly HashSet<string> _supportedCurrencies = settings.SupportedCurrencies;


    public ValidationResult Validate(PaymentRequest? payment)
    {
        List<string> errorMessages = new();

        if (payment is null)
        {
            errorMessages.Add(Errors.EmptyPaymentRequest);
            return new ValidationResult(false, errorMessages);
        }

        ValidateCardNumber(payment.CardNumber, errorMessages);
        ValidateCvv(payment.Cvv, errorMessages);
        ValidateExpiryDate(payment.ExpiryMonth, payment.ExpiryYear, errorMessages);
        ValidateAmount(payment.Amount, errorMessages);
        ValidateCurrency(payment.Currency, errorMessages);

        return new ValidationResult(errorMessages.Count == 0, errorMessages);
    }

    private void ValidateCardNumber(string number, List<string> errorMessages)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            errorMessages.Add(Errors.CardNumberRequired);
            return;
        }

        if (IsValidCardNumber(number) is false)
        {
            errorMessages.Add(Errors.CardNumberInvalid);
        }
    }

    private void ValidateCvv(string cardCvv, List<string> errorMessages)
    {
        if (string.IsNullOrWhiteSpace(cardCvv))
        {
            errorMessages.Add(Errors.CvvRequired);
            return;
        }

        if (IsValidCvvLength(cardCvv) is false)
        {
            errorMessages.Add(Errors.CvvInvalid);
        }
    }

    private void ValidateExpiryDate(int? cardExpiryMonth, int? cardExpiryYear, List<string> errorMessages)
    {
        if (ValidateExpiryMonth(cardExpiryMonth, errorMessages) is false ||
            ValidateExpiryYear(cardExpiryYear, errorMessages) is false)
        {
            return;
        }

        DateTime utcNow = DateTime.UtcNow;
        DateTime expiryDate = new(cardExpiryYear.Value, cardExpiryMonth.Value, 1);
        DateTime currentMonthDate = new(utcNow.Year, utcNow.Month, 1);

        if (expiryDate < currentMonthDate)
        {
            errorMessages.Add(Errors.CardExpired);
        }
    }

    private void ValidateAmount(long? amount, List<string> errorMessages)
    {
        if (amount.HasValue is false)
        {
            errorMessages.Add(Errors.AmountRequired);
            return;
        }

        if (amount.Value < MinAmount)
        {
            errorMessages.Add(Errors.AmountInvalid);
        }
    }

    private void ValidateCurrency(string currency, List<string> errorMessages)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            errorMessages.Add(Errors.CurrencyRequired);
            return;
        }

        if (currency.Length > CurrencyCharactersLength)
        {
            errorMessages.Add(Errors.CurrencyInvalid);
            return;
        }

        if (!_supportedCurrencies.Contains(currency))
        {
            errorMessages.Add(Errors.CurrencyUnsupported);
        }
    }

    private bool ValidateExpiryMonth(int? cardExpiryMonth, List<string> errorMessages)
    {
        if (cardExpiryMonth.HasValue is false)
        {
            errorMessages.Add(Errors.CardExpiryMonthRequired);
            return false;
        }

        if (cardExpiryMonth.Value is < MinExpiryMonth or > MaxExpiryMonth)
        {
            errorMessages.Add(Errors.CardExpiryMonthInvalid);
            return false;
        }

        return true;
    }

    private bool ValidateExpiryYear(int? cardExpiryYear, List<string> errorMessages)
    {
        if (cardExpiryYear.HasValue is false)
        {
            errorMessages.Add(Errors.CardExpiryYearRequired);
            return false;
        }

        if (cardExpiryYear.Value > MaxYear)
        {
            errorMessages.Add(Errors.CardExpiryYearInvalid);
            return false;
        }

        if (cardExpiryYear.Value < DateTime.UtcNow.Year)
        {
            errorMessages.Add(Errors.CardExpiryYearInvalid);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Determines whether the provided card number has digits only and allowed length
    /// </summary>
    /// <param name="number">The card number</param>
    /// <returns>True if the number is valid, otherwise False</returns>
    private static bool IsValidCardNumber(string number)
    {
        return number.Length is >= MinCardNumberLength and <= MaxCardNumberLength && IsValidNumber(number);
    }

    /// <summary>
    ///     Check if all characters are digits <paramref name="number" />
    /// </summary>
    /// <param name="number">The number</param>
    /// <returns>True if the number contains digits only, otherwise False.</returns>
    private static bool IsValidNumber(string number)
    {
        return number.All(Char.IsDigit);
    }

    /// <summary>
    ///     Validates whether the CVV is valid number and length
    /// </summary>
    /// <param name="cvv">The CVV</param>
    /// <returns>True if the CVV is valid, otherwise False</returns>
    private static bool IsValidCvvLength(string cvv)
    {
        return cvv.Length is >= MinCvvLength and <= MaxCvvLength && IsValidNumber(cvv);
    }
}