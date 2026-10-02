using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for Paymob payment gateway integration:
/// - 3-Step Checkout Session Generation (Auth -> Order -> PaymentKey -> Iframe URL)
/// - Cryptographic HMAC-SHA512 verification for secure Callbacks and Webhooks
/// </summary>
public interface IPaymobService
{
    /// <summary>
    /// Initiates a payment session with Paymob and generates the secured customer-facing Iframe URL.
    /// </summary>
    Task<string> InitiatePaymentAsync(Order order, PaymentMethod paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cryptographically validates the HMAC hash sent by Paymob in the Transaction Response Callback (GET).
    /// </summary>
    bool ValidateCallbackHmac(IDictionary<string, string> queryParams, string hmacToVerify);
}
