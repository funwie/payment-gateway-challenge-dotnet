namespace PaymentGateway.Api.Models.Responses;

public static class ErrorTypes
{
    public const string InternalError = "internal_error";
    public const string ValidationError = "validation_error";
}

public static class Errors
{
    public const string DataParsingError = "parsing_error";
    public const string ProcessingError = "processing_error";
    public const string AcquiringError = "acquiring_error";
    public const string AmountInvalid = "amount_invalid";
    public const string AmountRequired = "amount_required";
    public const string CurrencyRequired = "currency_required";
    public const string CurrencyInvalid = "currency_invalid";
    public const string CurrencyUnsupported = "currency_unsupported";
    public const string CardNumberRequired = "card_number_required";
    public const string CardNumberInvalid = "card_number_invalid";
    public const string CardExpiryMonthRequired = "card_expiry_month_required";
    public const string CardExpiryMonthInvalid = "card_expiry_month_invalid";
    public const string CardExpiryYearRequired = "card_expiry_year_required";
    public const string CardExpiryYearInvalid = "card_expiry_year_invalid";
    public const string CardExpired = "card_expired";
    public const string CvvInvalid = "cvv_invalid";
    public const string CvvRequired = "cvv_required";
    public const string EmptyPaymentRequest = "empty_payment_request";
}