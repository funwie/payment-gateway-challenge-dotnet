using System.Net;
using System.Net.Mime;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Storage;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Random _random = new();
    private readonly DateTime _now = DateTime.UtcNow;

    private readonly IPaymentsRepository _paymentsRepository = factory.Services.GetRequiredService<IPaymentsRepository>();
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            ExpiryYear = _random.Next(_now.Year, _now.Year + 10),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = "1234567890123456",
            Cvv = "123",
            Currency = "GBP",
            CreatedAt = _now
        };

        (PaymentResponse? expectedPayment, _) = Mapper.ToPaymentResponse(payment);
        _paymentsRepository.Add(payment);

        // Act
        var response = await _client.GetAsync($"/api/payments/{payment.Id}");
        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentResponse = SerializerSettings.Deserialize<PaymentResponse>(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(expectedPayment, paymentResponse);
    }

    [Fact]
    public async Task Returns404NotFoundIfPaymentDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Returns400BadRequestIfInvalidPaymentRequest()
    {
        // Arrange
        var request = new PaymentRequest { CardNumber = "123", Amount = 100, Currency = "GBP", Cvv = "123", ExpiryMonth = 4, ExpiryYear = 2025 };

        // Act
        var response = await _client.SendAsync(BuildPostRequest(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Returns201CreatedWithAuthorisedStatus()
    {
        var request = new PaymentRequest { CardNumber = "2222405343248877", Amount = 100, Currency = "GBP", Cvv = "123", ExpiryMonth = 4, ExpiryYear = 2025 };

        // Act
        var response = await _client.SendAsync(BuildPostRequest(request));
        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentResponse = SerializerSettings.Deserialize<PaymentResponse>(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentResponse.Status, PaymentStatus.Authorized);
    }

    [Fact]
    public async Task Returns201CreatedWithDeclinedStatus()
    {
        var request = new PaymentRequest { CardNumber = "2222405343248112", Cvv = "456", ExpiryMonth = 1, ExpiryYear = 2026, Amount = 60000, Currency = "USD"};

        // Act
        var response = await _client.SendAsync(BuildPostRequest(request));
        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentResponse = SerializerSettings.Deserialize<PaymentResponse>(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentResponse.Status, PaymentStatus.Declined);
    }

    private HttpRequestMessage BuildPostRequest(PaymentRequest paymentRequest)
    {
        var jsonBody = JsonConvert.SerializeObject(paymentRequest, SerializerSettings.DefaultJsonSerializerSettings());
        return new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json),
        };
    }

    [Fact]
    public async Task ListPaymentsSuccessfully()
    {
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            ExpiryYear = _random.Next(_now.Year, _now.Year + 10),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = "1234567890123456",
            Cvv = "123",
            Currency = "GBP",
            CreatedAt = _now
        };
        _paymentsRepository.Add(payment);
        var payment2 = payment with { Id = Guid.NewGuid(), Status = PaymentStatus.Declined.ToString() };
        _paymentsRepository.Add(payment2);

        // Act
        var response = await _client.GetAsync("/api/payments");
        var responseContent = await response.Content.ReadAsStringAsync();
        var payments = SerializerSettings.Deserialize<PaymentResponse[]>(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(payments);
        Assert.True(payments.Length >= 2);
    }
}