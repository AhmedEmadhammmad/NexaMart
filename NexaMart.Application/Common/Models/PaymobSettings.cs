namespace NexaMart.Application.Common.Models;

/// <summary>
/// Strongly-typed configuration options for Paymob Payment Gateway.
/// </summary>
public class PaymobSettings
{
    public const string SectionName = "PaymobSettings";

    public string BaseUrl { get; set; } = "https://accept.paymob.com";
    public string ApiKey { get; set; } = string.Empty;
    public int CardIntegrationId { get; set; }
    public int WalletIntegrationId { get; set; }
    public int IframeId { get; set; }
    public string HmacSecret { get; set; } = string.Empty;
}
