namespace NexaMart.Domain.Enums;

/// <summary>
/// Payment methods supported by NexaMart, ready for local and Paymob payment integrations.
/// </summary>
public enum PaymentMethod
{
    CashOnDelivery = 1,
    PaymobCard = 2,
    PaymobWallet = 3,
    PaymobKiosk = 4
}
