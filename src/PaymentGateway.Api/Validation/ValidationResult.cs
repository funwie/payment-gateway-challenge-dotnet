namespace PaymentGateway.Api.Validation;

public record ValidationResult(bool IsValid, List<string> ErrorMessages);