using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Enums;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Controller handling Paymob payment gateway callbacks and notifications:
/// - Browser Response Callback (GET): Validates HMAC signature and redirects customer to invoice on success.
/// - Server-to-Server Webhook (POST): Receives background transaction notifications from Paymob.
/// </summary>
public class PaymentController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IPaymobService _paymobService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IOrderService orderService,
        IPaymobService paymobService,
        ILogger<PaymentController> logger)
    {
        _orderService = orderService;
        _paymobService = paymobService;
        _logger = logger;
    }

    // =========================================================================
    // 1. TRANSACTION RESPONSE CALLBACK (GET from Paymob browser redirect)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        var queryParams = HttpContext.Request.Query
            .ToDictionary(k => k.Key, v => v.Value.ToString());

        var hmac = queryParams.GetValueOrDefault("hmac", string.Empty);
        var successStr = queryParams.GetValueOrDefault("success", "false");
        var transactionId = queryParams.GetValueOrDefault("id", string.Empty);
        var merchantOrderId = queryParams.GetValueOrDefault("merchant_order_id", string.Empty);

        _logger.LogInformation("Paymob Callback received. MerchantOrder: {MerchantOrder}, TxId: {TxId}, Success: {Success}",
            merchantOrderId, transactionId, successStr);

        // Cryptographically verify HMAC-SHA512 signature
        var isValidHmac = _paymobService.ValidateCallbackHmac(queryParams, hmac);
        if (!isValidHmac)
        {
            _logger.LogWarning("Security Violation: Paymob Callback HMAC verification failed for MerchantOrder: {OrderNumber}", merchantOrderId);
            TempData["ErrorMessage"] = "Payment security verification failed. Please contact customer support.";
            return RedirectToAction("Index", "Cart");
        }

        var isSuccessful = string.Equals(successStr, "true", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(merchantOrderId))
        {
            var order = await _orderService.GetOrderByNumberAsync(merchantOrderId, HttpContext.RequestAborted);
            if (order != null)
            {
                if (isSuccessful)
                {
                    // Transition order to Paid & Confirmed, which dispatches the official invoice email automatically
                    await _orderService.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Paid, transactionId, HttpContext.RequestAborted);
                    TempData["SuccessMessage"] = $"Payment captured successfully! Paymob Reference: #{transactionId}";
                    return RedirectToAction("Invoice", "Orders", new { id = order.Id });
                }
                else
                {
                    await _orderService.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Failed, transactionId, HttpContext.RequestAborted);
                    TempData["ErrorMessage"] = "Your payment transaction was declined or cancelled. Please review your card details and try again.";
                    return RedirectToAction("Checkout", "Cart");
                }
            }
        }

        TempData["ErrorMessage"] = "Order not found or invalid transaction reference.";
        return RedirectToAction("Index", "Cart");
    }

    // =========================================================================
    // 2. TRANSACTION PROCESSED WEBHOOK (POST from Paymob server)
    // =========================================================================

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Webhook([FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            if (payload.TryGetProperty("obj", out var objElement))
            {
                var isSuccess = objElement.TryGetProperty("success", out var s) && s.GetBoolean();
                var txId = objElement.TryGetProperty("id", out var idElem) ? idElem.ToString() : null;

                if (objElement.TryGetProperty("order", out var orderElem) &&
                    orderElem.TryGetProperty("merchant_order_id", out var merchantOrderElem))
                {
                    var merchantOrderId = merchantOrderElem.GetString();
                    if (!string.IsNullOrWhiteSpace(merchantOrderId))
                    {
                        var order = await _orderService.GetOrderByNumberAsync(merchantOrderId, HttpContext.RequestAborted);
                        if (order != null && isSuccess && order.PaymentStatus != PaymentStatus.Paid)
                        {
                            await _orderService.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Paid, txId, HttpContext.RequestAborted);
                            _logger.LogInformation("Webhook: Order #{OrderNumber} marked as Paid from Paymob background notification.", merchantOrderId);
                        }
                    }
                }
            }

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Paymob Webhook.");
            return Ok(); // Always return 200 to acknowledge receipt to Paymob
        }
    }
}
