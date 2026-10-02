using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for dispatching transactional notification emails:
/// - 6-Digit OTP verification on registration
/// - 6-Digit OTP password reset
/// - Official order confirmation invoices
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendEmailConfirmationOtpAsync(string toEmail, string customerName, string otpCode, CancellationToken cancellationToken = default);
    Task SendPasswordResetOtpAsync(string toEmail, string customerName, string otpCode, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationEmailAsync(Order order, CancellationToken cancellationToken = default);
    Task SendOrderCancellationEmailAsync(Order order, string reason, CancellationToken cancellationToken = default);
}
