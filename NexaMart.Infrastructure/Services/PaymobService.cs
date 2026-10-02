using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaMart.Application.Common.Models;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Infrastructure.Services;

/// <summary>
/// Paymob Payment Gateway Implementation:
/// Executes the 3-step authentication, order registration, and payment key request to generate secure Iframe URLs.
/// Provides HMAC-SHA512 cryptographic verification for transaction response callbacks.
/// </summary>
public class PaymobService : IPaymobService
{
    private readonly HttpClient _httpClient;
    private readonly PaymobSettings _settings;
    private readonly ILogger<PaymobService> _logger;

    public PaymobService(
        HttpClient httpClient,
        IOptions<PaymobSettings> settings,
        ILogger<PaymobService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _settings.BaseUrl = "https://accept.paymob.com";
        }
    }

    public async Task<string> InitiatePaymentAsync(Order order, PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Paymob ApiKey is not configured in appsettings.json.");
        }

        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        _logger.LogInformation("Initiating Paymob payment session for order #{OrderNumber}, Total: {TotalAmount} {Currency}",
            order.OrderNumber, order.TotalAmount, order.Currency);

        // ---------------------------------------------------------------------
        // Step 1: Request Authentication Token
        // ---------------------------------------------------------------------
        var authResponse = await _httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/auth/tokens",
            new { api_key = _settings.ApiKey },
            cancellationToken);

        if (!authResponse.IsSuccessStatusCode)
        {
            var errorBody = await authResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Paymob Auth Token request failed ({StatusCode}): {Error}", authResponse.StatusCode, errorBody);
            throw new InvalidOperationException("Failed to authenticate with Paymob payment gateway.");
        }

        using var authJson = await JsonDocument.ParseAsync(await authResponse.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var authToken = authJson.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Paymob auth token response was null.");

        // ---------------------------------------------------------------------
        // Step 2: Register Order with Paymob (Amount strictly converted to Cents)
        // ---------------------------------------------------------------------
        long amountCents = (long)Math.Round(order.TotalAmount * 100m, MidpointRounding.AwayFromZero);
        var currency = string.IsNullOrWhiteSpace(order.Currency) ? "EGP" : order.Currency.Trim();

        var orderPayload = new
        {
            auth_token = authToken,
            delivery_needed = "false",
            amount_cents = amountCents.ToString(),
            currency,
            merchant_order_id = order.OrderNumber,
            items = Array.Empty<object>()
        };

        var orderResponse = await _httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/ecommerce/orders",
            orderPayload,
            cancellationToken);

        if (!orderResponse.IsSuccessStatusCode)
        {
            var errorBody = await orderResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Paymob Order Registration failed ({StatusCode}): {Error}", orderResponse.StatusCode, errorBody);
            throw new InvalidOperationException("Failed to register order with Paymob.");
        }

        using var orderJson = await JsonDocument.ParseAsync(await orderResponse.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var paymobOrderId = orderJson.RootElement.GetProperty("id").GetInt64();

        // ---------------------------------------------------------------------
        // Step 3: Request Payment Key Token with Customer Billing Data
        // ---------------------------------------------------------------------
        var integrationId = paymentMethod == PaymentMethod.PaymobWallet && _settings.WalletIntegrationId > 0
            ? _settings.WalletIntegrationId
            : _settings.CardIntegrationId;

        var names = (order.CustomerName ?? "Customer").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = names.Length > 0 ? names[0] : "Customer";
        var lastName = names.Length > 1 ? names[1] : "Customer";

        var billingData = new
        {
            apartment = "NA",
            email = !string.IsNullOrWhiteSpace(order.CustomerEmail) ? order.CustomerEmail : "customer@nexamart.com",
            floor = "NA",
            first_name = firstName,
            street = !string.IsNullOrWhiteSpace(order.ShippingAddress) ? order.ShippingAddress : "Delivery Street",
            building = "NA",
            phone_number = !string.IsNullOrWhiteSpace(order.CustomerPhone) ? order.CustomerPhone : "+201000000000",
            shipping_method = "PKG",
            postal_code = !string.IsNullOrWhiteSpace(order.PostalCode) ? order.PostalCode : "NA",
            city = !string.IsNullOrWhiteSpace(order.City) ? order.City : "Cairo",
            country = "EGY",
            last_name = lastName,
            state = !string.IsNullOrWhiteSpace(order.State) ? order.State : "Cairo"
        };

        var paymentKeyPayload = new
        {
            auth_token = authToken,
            amount_cents = amountCents.ToString(),
            expiration = 3600,
            order_id = paymobOrderId.ToString(),
            billing_data = billingData,
            currency,
            integration_id = integrationId
        };

        var paymentKeyResponse = await _httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/acceptance/payment_keys",
            paymentKeyPayload,
            cancellationToken);

        if (!paymentKeyResponse.IsSuccessStatusCode)
        {
            var errorBody = await paymentKeyResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Paymob Payment Key request failed ({StatusCode}): {Error}", paymentKeyResponse.StatusCode, errorBody);
            throw new InvalidOperationException("Failed to generate payment key with Paymob.");
        }

        using var paymentKeyJson = await JsonDocument.ParseAsync(await paymentKeyResponse.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var paymentKeyToken = paymentKeyJson.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Paymob payment key token was null.");

        // ---------------------------------------------------------------------
        // Step 4: Construct and Return the Customer Payment Iframe URL
        // ---------------------------------------------------------------------
        var iframeUrl = $"{baseUrl}/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentKeyToken}";
        _logger.LogInformation("Successfully generated Paymob Iframe URL for order #{OrderNumber}", order.OrderNumber);

        return iframeUrl;
    }

    public bool ValidateCallbackHmac(IDictionary<string, string> queryParams, string hmacToVerify)
    {
        if (string.IsNullOrWhiteSpace(hmacToVerify) || string.IsNullOrWhiteSpace(_settings.HmacSecret))
        {
            return false;
        }

        // Paymob standard alphabetical concatenation list for Transaction Response Callback (GET)
        var orderedKeys = new[]
        {
            "amount_cents",
            "created_at",
            "currency",
            "error_occured",
            "has_parent_transaction",
            "id",
            "integration_id",
            "is_3d_secure",
            "is_auth",
            "is_capture",
            "is_refunded",
            "is_standalone_payment",
            "is_voided",
            "order",
            "owner",
            "pending",
            "source_data.pan",
            "source_data.sub_type",
            "source_data.type",
            "success"
        };

        var sb = new StringBuilder();
        foreach (var key in orderedKeys)
        {
            if (queryParams.TryGetValue(key, out var val))
            {
                sb.Append(val);
            }
        }

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HmacSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        var computedHmac = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return string.Equals(computedHmac, hmacToVerify, StringComparison.OrdinalIgnoreCase);
    }
}
