using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class PaymentsController(IPaymentService paymentService, IPaymentRequestValidator requestValidator) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaymentAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        (PaymentResponse? payment, ErrorResponse? errorResponse) =
            await paymentService.GetByIdAsync(id, cancellationToken);

        if (errorResponse is not null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        if (payment is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<IActionResult> AuthorizePaymentAsync([FromBody] PaymentRequest paymentRequest,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = requestValidator.Validate(paymentRequest);
        if (validationResult.IsValid is false)
        {
            return new BadRequestObjectResult(new ErrorResponse(ErrorTypes.ValidationError,
                validationResult.ErrorMessages));
        }

        (PaymentResponse? payment, ErrorResponse? errorResponse) =
            await paymentService.AuthorizeAsync(paymentRequest, cancellationToken);

        if (errorResponse is not null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        if (payment is null)
        {
            return new NotFoundResult();
        }

        return Created($"/api/payments/{payment.Id}", payment);
    }

    [HttpGet]
    public async Task<IActionResult> ListPaymentsAsync(CancellationToken cancellationToken)
    {
        (IEnumerable<PaymentResponse> payments, ErrorResponse? errorResponse) =
            await paymentService.ListAsync(cancellationToken);

        return errorResponse is not null
            ? StatusCode(StatusCodes.Status500InternalServerError, errorResponse)
            : new OkObjectResult(payments);
    }
}