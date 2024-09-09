using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.UnitTests;

public class PaymentRequestValidatorTests
{
    private readonly PaymentRequestValidator _sut = new(new Settings{SupportedCurrencies = ["GBP"] });
    private readonly DateTime _now = DateTime.UtcNow;

    [Fact]
    public void NullPaymentRequest()
    {
        // Arrange
        PaymentRequest? paymentRequest = null;

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("empty_payment_request", result.ErrorMessages.Single());
    }

    [Fact]
    public void ValidCardNumber()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with {CardNumber = "1234567890123456" };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void EmptyCardNumber()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { CardNumber = ""} ;

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_number_required", result.ErrorMessages.Single());
    }

    [Theory]
    [InlineData("12345678901234567890")]
    [InlineData("1234567890123")]
    [InlineData("1234567890123DD")]
    [InlineData("A1234567 890123")]
    public void InvalidCardNumber(string invalidCardNumber)
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { CardNumber = invalidCardNumber };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_number_invalid", result.ErrorMessages.Single());
    }

    [Fact]
    public void ValidExpiryMonth()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryMonth = 12 };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }


    [Fact]
    public void EmptyExpiryMonth()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryMonth = null };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_expiry_month_required", result.ErrorMessages.Single());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public void InvalidExpiryMonth(int expiryMonth)
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryMonth = expiryMonth };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_expiry_month_invalid", result.ErrorMessages.Single());
    }

    [Fact]
    public void ValidExpiryYear()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryYear = _now.Year + 4 };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }


    [Fact]
    public void EmptyExpiryYear()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryYear = null };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_expiry_year_required", result.ErrorMessages.Single());
    }

    [Theory]
    [InlineData(2000)]
    [InlineData(-2020)]
    [InlineData(0)]
    [InlineData(123456789)]
    public void InvalidExpiryYear(int expiryYear)
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryYear = expiryYear };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_expiry_year_invalid", result.ErrorMessages.Single());
    }

    [Fact]
    public void ValidExpiryDate()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryMonth = _now.Month, ExpiryYear = _now.Year };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void InvalidExpiryDate()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { ExpiryMonth = _now.Month - 1, ExpiryYear = _now.Year };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("card_expired", result.ErrorMessages.Single());
    }

    [Fact]
    public void ValidCurrency()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Currency = "GBP"};

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void EmptyCurrency()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Currency = "" };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("currency_required", result.ErrorMessages.Single());
    }

    [Fact]
    public void InvalidCurrency()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Currency = "GBPINVALID" };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("currency_invalid", result.ErrorMessages.Single());
    }

    [Fact]
    public void UnsupportedCurrency()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Currency = "EUR" };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("currency_unsupported", result.ErrorMessages.Single());
    }

    [Fact]
    public void ValidAmount()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Amount = long.MaxValue };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void EmptyAmount()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Amount = null };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("amount_required", result.ErrorMessages.Single());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidAmount(long invalidAmount)
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Amount = invalidAmount };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("amount_invalid", result.ErrorMessages.Single());
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    public void ValidCardCvv(string validCvv)
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Cvv = validCvv };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void EmptyCardCvv()
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Cvv = "" };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("cvv_required", result.ErrorMessages.Single());
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("ABCDE")]
    [InlineData("123A")]
    [InlineData("123 ")]
    public void InvalidCardCvv(string invalidCardCvv)
    {
        // Arrange
        var paymentRequest = ValidPaymentRequest with { Cvv = invalidCardCvv };

        // Act
        var result = _sut.Validate(paymentRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("cvv_invalid", result.ErrorMessages.Single());
    }

    private PaymentRequest ValidPaymentRequest => new PaymentRequest
    {
        CardNumber = "1234567890123456",
        ExpiryMonth = 12,
        ExpiryYear = _now.Year,
        Amount = 100,
        Currency = "GBP",
        Cvv = "123"
    };
}