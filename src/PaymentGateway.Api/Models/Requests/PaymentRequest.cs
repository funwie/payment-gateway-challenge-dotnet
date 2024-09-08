﻿namespace PaymentGateway.Api.Models.Requests;

public record PaymentRequest
{
    public string CardNumber { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string Currency { get; set; }
    public long? Amount { get; set; }
    public string Cvv { get; set; }
}