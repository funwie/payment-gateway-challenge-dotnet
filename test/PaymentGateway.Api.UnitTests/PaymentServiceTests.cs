using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Storage;

namespace PaymentGateway.Api.UnitTests;

public class PaymentServiceTests
{
    private readonly IAcquirerClient _acquirerClient = Substitute.For<IAcquirerClient>();
    private readonly ILogger<PaymentService> _logger = Substitute.For<ILogger<PaymentService>>();
    private readonly IPaymentsRepository _paymentRepository = Substitute.For<IPaymentsRepository>();
    private readonly IPaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(_acquirerClient, _paymentRepository, _logger);
    }

    [Fact]
    public async Task CallsAcquirerWithCorrectPayload()
    {
        // Arrange
        var request = ValidPaymentRequest;
        _acquirerClient
            .AcquireAsync(Arg.Any<AcquirerRequest>(), CancellationToken.None)
            .Returns(new AcquirerResponse(true, "10000"));

        // Act
        (PaymentResponse? paymentResponse, ErrorResponse? errorResponse) = await _sut.AuthorizeAsync(request, CancellationToken.None);

        // Assert
        Assert.Null(errorResponse);
        Assert.NotNull(paymentResponse);
        await _acquirerClient.Received().AcquireAsync(Arg.Is<AcquirerRequest>(p => 
            p.CardNumber == request.CardNumber &&
            p.Cvv == request.Cvv &&
            p.Amount == request.Amount!.Value &&
            p.Currency == request.Currency &&
            p.ExpiryDate == $"{request.ExpiryMonth}/{request.ExpiryYear}"), CancellationToken.None);
    }

    [Theory]
    [InlineData(true, PaymentStatus.Authorized)]
    [InlineData(false, PaymentStatus.Declined)]
    public async Task StoresPaymentWithCorrectStatus(bool authorized, PaymentStatus expectedStatus)
    {
        // Arrange
        var request = ValidPaymentRequest;
        _acquirerClient
            .AcquireAsync(Arg.Any<AcquirerRequest>(), CancellationToken.None)
            .Returns(new AcquirerResponse(authorized, "10000"));

        // Act
        (PaymentResponse? paymentResponse, ErrorResponse? errorResponse) = await _sut.AuthorizeAsync(ValidPaymentRequest, CancellationToken.None);

        // Assert
        Assert.Null(errorResponse);
        Assert.NotNull(paymentResponse);
        Assert.Equal(expectedStatus, paymentResponse.Status);
        _paymentRepository.Received().Add(Arg.Is<Payment>(p =>
            p.Status == expectedStatus.ToString() &&
            p.AuthorizationResult!.Authorized == authorized &&
            p.AuthorizationResult.AuthorizationCode == "10000" &&
            p.CardNumber == request.CardNumber &&
            p.Cvv == request.Cvv &&
            p.ExpiryYear == request.ExpiryYear &&
            p.ExpiryMonth == request.ExpiryMonth &&
            p.Amount == request.Amount!.Value &&
            p.Currency == request.Currency &&
            p.Id != Guid.Empty && 
            p.CreatedAt != default));
    }


    [Fact]
    public async Task HandleAcquirerCallException()
    {
        // Arrange
        _acquirerClient
            .AcquireAsync(Arg.Any<AcquirerRequest>(), CancellationToken.None)
            .ThrowsAsync(new AcquirerException("failure"));

        // Act
        (PaymentResponse? paymentResponse, ErrorResponse? errorResponse) = await _sut.AuthorizeAsync(ValidPaymentRequest, CancellationToken.None);

        // Assert
        Assert.Null(paymentResponse);
        Assert.NotNull(errorResponse);
        Assert.Equal("internal_error", errorResponse.ErrorType);
        Assert.Equal("acquiring_error", errorResponse.ErrorMessages.Single());
    }


    [Fact]
    public async Task HandleInternalException()
    {
        // Arrange
        _acquirerClient
            .AcquireAsync(Arg.Any<AcquirerRequest>(), CancellationToken.None)
            .ThrowsAsync(new Exception("failure"));

        // Act
        (PaymentResponse? paymentResponse, ErrorResponse? errorResponse) = await _sut.AuthorizeAsync(ValidPaymentRequest, CancellationToken.None);

        // Assert
        Assert.Null(paymentResponse);
        Assert.NotNull(errorResponse);
        Assert.Equal("internal_error", errorResponse.ErrorType);
        Assert.Equal("processing_error", errorResponse.ErrorMessages.Single());
    }

    private PaymentRequest ValidPaymentRequest => new PaymentRequest
    {
        CardNumber = "1234567890123456",
        ExpiryMonth = 12,
        ExpiryYear = 2100,
        Amount = 100,
        Currency = "GBP",
        Cvv = "123"
    };
}