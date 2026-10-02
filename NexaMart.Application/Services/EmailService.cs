using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Services;

/// <summary>
/// Enterprise email service implementing transactional notifications:
/// - 6-Digit Email Verification OTP
/// - 6-Digit Password Reset OTP
/// - Full Order Confirmation Invoice
/// Includes graceful fallback logging for local development.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _configuration["EmailSettings:Host"];
        var portStr = _configuration["EmailSettings:Port"];
        var username = _configuration["EmailSettings:UserName"];
        var password = _configuration["EmailSettings:Password"];
        var enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) && ssl;
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@nexamart.com";
        var fromName = _configuration["EmailSettings:FromName"] ?? "NexaMart Official";

        int port = int.TryParse(portStr, out var p) ? p : 587;

        // If SMTP credentials are not configured or set to placeholder, log securely in development
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || host.Contains("example.com"))
        {
            _logger.LogInformation("================================================================================");
            _logger.LogInformation("📧 [DEV EMAIL SIMULATION]");
            _logger.LogInformation("To: {ToEmail}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("================================================================================");
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password),
                Timeout = 10000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("✅ Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "⚠️ Failed to send email to {ToEmail}. Fallback to simulated delivery.", toEmail);
        }
    }

    public async Task SendEmailConfirmationOtpAsync(string toEmail, string customerName, string otpCode, CancellationToken cancellationToken = default)
    {
        var subject = $"NexaMart - رمز تأكيد حسابك: {otpCode}";
        var html = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; }}
        .card {{ max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); border: 1px solid #e2e8f0; }}
        .header {{ background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); padding: 32px 24px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 26px; font-weight: 800; letter-spacing: -0.5px; }}
        .header p {{ margin: 8px 0 0; color: #94a3b8; font-size: 14px; }}
        .content {{ padding: 36px 28px; text-align: center; }}
        .greeting {{ font-size: 18px; color: #1e293b; font-weight: 700; margin-bottom: 12px; }}
        .msg {{ font-size: 15px; color: #64748b; line-height: 1.6; margin-bottom: 24px; }}
        .otp-box {{ background: #f1f5f9; border: 2px dashed #0284c7; border-radius: 12px; padding: 20px; margin: 24px 0; display: inline-block; min-width: 220px; }}
        .otp-code {{ font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #0369a1; margin: 0; font-family: monospace; }}
        .expiry {{ font-size: 13px; color: #e11d48; font-weight: 600; margin-top: 8px; }}
        .security-note {{ font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; padding-top: 16px; margin-top: 24px; }}
        .footer {{ background: #f8fafc; padding: 18px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <h1>NexaMart</h1>
            <p>منصة التجارة الإلكترونية العصرية</p>
        </div>
        <div class=""content"">
            <div class=""greeting"">مرحباً {WebUtility.HtmlEncode(customerName)} 👋</div>
            <div class=""msg"">شكراً لانضمامك إلى NexaMart! لتفعيل حسابك والبدء في التسوق، يرجى استخدام رمز التحقق (OTP) التالي:</div>
            <div class=""otp-box"">
                <div class=""otp-code"">{otpCode}</div>
                <div class=""expiry"">⏱️ الرمز صالح لمدة 15 دقيقة فقط</div>
            </div>
            <div class=""security-note"">
                ⚠️ يرجى عدم مشاركة هذا الرمز مع أي شخص. فريق NexaMart لن يطلب منك هذا الرمز أبداً.
            </div>
        </div>
        <div class=""footer"">
            © {DateTime.UtcNow.Year} NexaMart. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, html, cancellationToken);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string customerName, string otpCode, CancellationToken cancellationToken = default)
    {
        var subject = $"NexaMart - رمز استعادة كلمة المرور: {otpCode}";
        var html = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; }}
        .card {{ max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); border: 1px solid #e2e8f0; }}
        .header {{ background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); padding: 32px 24px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 26px; font-weight: 800; letter-spacing: -0.5px; }}
        .content {{ padding: 36px 28px; text-align: center; }}
        .greeting {{ font-size: 18px; color: #1e293b; font-weight: 700; margin-bottom: 12px; }}
        .msg {{ font-size: 15px; color: #64748b; line-height: 1.6; margin-bottom: 24px; }}
        .otp-box {{ background: #fef2f2; border: 2px dashed #ef4444; border-radius: 12px; padding: 20px; margin: 24px 0; display: inline-block; min-width: 220px; }}
        .otp-code {{ font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #dc2626; margin: 0; font-family: monospace; }}
        .expiry {{ font-size: 13px; color: #dc2626; font-weight: 600; margin-top: 8px; }}
        .security-note {{ font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; padding-top: 16px; margin-top: 24px; }}
        .footer {{ background: #f8fafc; padding: 18px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <h1>NexaMart</h1>
            <p>طلب إعادة تعيين كلمة المرور</p>
        </div>
        <div class=""content"">
            <div class=""greeting"">مرحباً {WebUtility.HtmlEncode(customerName)} 👋</div>
            <div class=""msg"">لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك. استخدم رمز التحقق التالي لإتمام التغيير:</div>
            <div class=""otp-box"">
                <div class=""otp-code"">{otpCode}</div>
                <div class=""expiry"">⏱️ الرمز صالح لمدة 15 دقيقة فقط</div>
            </div>
            <div class=""security-note"">
                إذا لم تطلب هذا الرمز، يمكنك تجاهل هذا البريد بأمان وحسابك في أمان تام.
            </div>
        </div>
        <div class=""footer"">
            © {DateTime.UtcNow.Year} NexaMart. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, html, cancellationToken);
    }

    public async Task SendOrderConfirmationEmailAsync(Order order, CancellationToken cancellationToken = default)
    {
        var recipientEmail = !string.IsNullOrWhiteSpace(order.CustomerEmail) 
            ? order.CustomerEmail 
            : (order.User != null ? order.User.Email : string.Empty);

        if (string.IsNullOrWhiteSpace(recipientEmail)) return;

        var customerName = !string.IsNullOrWhiteSpace(order.CustomerName) 
            ? order.CustomerName 
            : (order.User != null ? order.User.FullName : "Customer");

        var subject = $"NexaMart - تأكيد استلام طلبك #{order.OrderNumber}";

        var rowsHtml = string.Empty;
        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                rowsHtml += $@"
                <tr>
                    <td style=""padding: 12px; border-bottom: 1px solid #e2e8f0; text-align: right;"">
                        <strong>{WebUtility.HtmlEncode(item.ProductName)}</strong>
                    </td>
                    <td style=""padding: 12px; border-bottom: 1px solid #e2e8f0; text-align: center;"">
                        {item.Quantity}
                    </td>
                    <td style=""padding: 12px; border-bottom: 1px solid #e2e8f0; text-align: left;"">
                        {item.UnitPrice:N2} {order.Currency}
                    </td>
                    <td style=""padding: 12px; border-bottom: 1px solid #e2e8f0; text-align: left; font-weight: bold;"">
                        {item.TotalPrice:N2} {order.Currency}
                    </td>
                </tr>";
            }
        }

        var html = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; }}
        .card {{ max-width: 640px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); border: 1px solid #e2e8f0; }}
        .header {{ background: linear-gradient(135deg, #0284c7 0%, #0369a1 100%); padding: 32px 24px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 26px; font-weight: 800; }}
        .order-badge {{ display: inline-block; background: rgba(255,255,255,0.2); border-radius: 8px; padding: 6px 14px; font-weight: 700; margin-top: 10px; font-size: 15px; }}
        .content {{ padding: 32px 24px; }}
        .greeting {{ font-size: 18px; color: #1e293b; font-weight: 700; margin-bottom: 10px; text-align: right; }}
        .msg {{ font-size: 14px; color: #64748b; line-height: 1.6; margin-bottom: 20px; text-align: right; }}
        .table-container {{ width: 100%; border-collapse: collapse; margin-top: 15px; margin-bottom: 25px; }}
        .table-header {{ background: #f1f5f9; }}
        .table-header th {{ padding: 12px; font-size: 13px; color: #475569; }}
        .financial-summary {{ background: #f8fafc; border-radius: 12px; padding: 18px; border: 1px solid #e2e8f0; }}
        .summary-row {{ display: flex; justify-content: space-between; padding: 6px 0; font-size: 14px; color: #475569; }}
        .summary-total {{ display: flex; justify-content: space-between; padding: 12px 0 0; font-size: 18px; font-weight: 800; color: #0f172a; border-top: 2px solid #cbd5e1; }}
        .details-box {{ background: #f1f5f9; border-radius: 12px; padding: 16px; margin-top: 20px; text-align: right; font-size: 13px; color: #334155; }}
        .footer {{ background: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <h1>NexaMart</h1>
            <div class=""order-badge"">رقم الطلب: {order.OrderNumber}</div>
        </div>
        <div class=""content"">
            <div class=""greeting"">شكراً لك يا {WebUtility.HtmlEncode(customerName)} على طلبك! 🎉</div>
            <div class=""msg"">تم تأكيد استلام طلبك بنجاح وجاري تجهيزه للشحن إلى عنوانك. فيما يلي الفاتورة والتفاصيل المالية:</div>

            <table class=""table-container"">
                <thead class=""table-header"">
                    <tr>
                        <th style=""text-align: right;"">المنتج</th>
                        <th style=""text-align: center;"">الكمية</th>
                        <th style=""text-align: left;"">سعر الوحدة</th>
                        <th style=""text-align: left;"">الإجمالي</th>
                    </tr>
                </thead>
                <tbody>
                    {rowsHtml}
                </tbody>
            </table>

            <div class=""financial-summary"">
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr>
                        <td style=""padding: 6px 0; color: #475569; text-align: right;"">المجموع الفرعي:</td>
                        <td style=""padding: 6px 0; font-weight: 600; text-align: left;"">{order.SubTotal:N2} {order.Currency}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 6px 0; color: #475569; text-align: right;"">مصاريف الشحن:</td>
                        <td style=""padding: 6px 0; font-weight: 600; text-align: left;"">{(order.ShippingCost > 0 ? $"{order.ShippingCost:N2} {order.Currency}" : "مجاني")}</td>
                    </tr>
                    {(order.TaxAmount > 0 ? $@"
                    <tr>
                        <td style=""padding: 6px 0; color: #475569; text-align: right;"">ضريبة القيمة المضافة:</td>
                        <td style=""padding: 6px 0; font-weight: 600; text-align: left;"">{order.TaxAmount:N2} {order.Currency}</td>
                    </tr>" : "")}
                    {(order.DiscountAmount > 0 ? $@"
                    <tr>
                        <td style=""padding: 6px 0; color: #16a34a; text-align: right;"">الخصم المطبق:</td>
                        <td style=""padding: 6px 0; font-weight: 600; color: #16a34a; text-align: left;"">-{order.DiscountAmount:N2} {order.Currency}</td>
                    </tr>" : "")}
                    <tr style=""border-top: 2px solid #cbd5e1;"">
                        <td style=""padding: 12px 0 0; font-size: 17px; font-weight: 800; color: #0f172a; text-align: right;"">الإجمالي الكلي:</td>
                        <td style=""padding: 12px 0 0; font-size: 17px; font-weight: 800; color: #0284c7; text-align: left;"">{order.TotalAmount:N2} {order.Currency}</td>
                    </tr>
                </table>
            </div>

            <div class=""details-box"">
                <strong>📍 عنوان التوصيل:</strong> {WebUtility.HtmlEncode(order.ShippingAddress)}, {WebUtility.HtmlEncode(order.City)}<br>
                <strong>💳 طريقة الدفع:</strong> {order.PaymentMethod} | <strong>حالة الدفع:</strong> {order.PaymentStatus}<br>
                <strong>📞 هاتف التواصل:</strong> {WebUtility.HtmlEncode(order.CustomerPhone)}
            </div>
        </div>
        <div class=""footer"">
            © {DateTime.UtcNow.Year} NexaMart. في حال كان لديك أي استفسار يرجى الرد على هذا البريد الإلكتروني.
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, html, cancellationToken);
    }

    public async Task SendOrderCancellationEmailAsync(Order order, string reason, CancellationToken cancellationToken = default)
    {
        var recipientEmail = !string.IsNullOrWhiteSpace(order.CustomerEmail) 
            ? order.CustomerEmail 
            : (order.User != null ? order.User.Email : string.Empty);

        if (string.IsNullOrWhiteSpace(recipientEmail)) return;

        var customerName = !string.IsNullOrWhiteSpace(order.CustomerName) 
            ? order.CustomerName 
            : (order.User != null ? order.User.FullName : "Customer");

        var subject = $"NexaMart - تأكيد إلغاء الطلب #{order.OrderNumber}";

        var rowsHtml = string.Empty;
        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                rowsHtml += $@"
                <tr>
                    <td style=""padding: 12px; border-bottom: 1px solid #fee2e2; text-align: right;"">
                        <strong>{WebUtility.HtmlEncode(item.ProductName)}</strong>
                    </td>
                    <td style=""padding: 12px; border-bottom: 1px solid #fee2e2; text-align: center;"">
                        {item.Quantity}
                    </td>
                    <td style=""padding: 12px; border-bottom: 1px solid #fee2e2; text-align: left;"">
                        {item.UnitPrice:N2} {order.Currency}
                    </td>
                    <td style=""padding: 12px; border-bottom: 1px solid #fee2e2; text-align: left; font-weight: bold;"">
                        {item.TotalPrice:N2} {order.Currency}
                    </td>
                </tr>";
            }
        }

        var html = $@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; }}
        .card {{ max-width: 640px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); border: 1px solid #e2e8f0; }}
        .header {{ background: linear-gradient(135deg, #b91c1c 0%, #991b1b 100%); padding: 32px 24px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 26px; font-weight: 800; }}
        .order-badge {{ display: inline-block; background: rgba(255,255,255,0.2); border-radius: 8px; padding: 6px 14px; font-weight: 700; margin-top: 10px; font-size: 15px; }}
        .content {{ padding: 32px 24px; }}
        .greeting {{ font-size: 18px; color: #1e293b; font-weight: 700; margin-bottom: 10px; text-align: right; }}
        .msg {{ font-size: 14px; color: #64748b; line-height: 1.6; margin-bottom: 20px; text-align: right; }}
        .cancellation-box {{ background: #fef2f2; border: 1px solid #fecaca; border-radius: 12px; padding: 18px; margin: 20px 0; text-align: right; color: #991b1b; }}
        .cancellation-title {{ font-weight: 800; font-size: 15px; margin-bottom: 6px; display: flex; align-items: center; gap: 8px; }}
        .table-container {{ width: 100%; border-collapse: collapse; margin-top: 15px; margin-bottom: 25px; }}
        .table-header {{ background: #fef2f2; }}
        .table-header th {{ padding: 12px; font-size: 13px; color: #991b1b; }}
        .financial-summary {{ background: #f8fafc; border-radius: 12px; padding: 18px; border: 1px solid #e2e8f0; }}
        .footer {{ background: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <h1>NexaMart</h1>
            <div class=""order-badge"">إلغاء الطلب: {order.OrderNumber}</div>
        </div>
        <div class=""content"">
            <div class=""greeting"">مرحباً {WebUtility.HtmlEncode(customerName)}،</div>
            <div class=""msg"">نود إعلامك بأنه تم تأكيد إلغاء طلبك رقم <strong>#{order.OrderNumber}</strong> بنجاح.</div>

            <div class=""cancellation-box"">
                <div class=""cancellation-title"">⚠️ سبب الإلغاء المسجل:</div>
                <div style=""font-size: 14px; line-height: 1.5;"">{WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(reason) ? "تم الإلغاء بناءً على رغبة العميل" : reason)}</div>
            </div>

            <p style=""font-size: 13px; color: #64748b; line-height: 1.6; text-align: right;"">
                تمت إعادة المنتجات إلى رصيد المخزون، وفي حال كنت قد قمت بسداد المبلغ إلكترونياً عبر البطاقة، سيتم رد المبلغ إلى حسابك البنكي وفق سياسات البنك المصدر (خلال 5-14 يوم عمل).
            </p>

            <table class=""table-container"">
                <thead class=""table-header"">
                    <tr>
                        <th style=""text-align: right;"">المنتج الملغي</th>
                        <th style=""text-align: center;"">الكمية</th>
                        <th style=""text-align: left;"">سعر الوحدة</th>
                        <th style=""text-align: left;"">الإجمالي</th>
                    </tr>
                </thead>
                <tbody>
                    {rowsHtml}
                </tbody>
            </table>

            <div class=""financial-summary"">
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr>
                        <td style=""padding: 8px 0; font-size: 16px; font-weight: 800; color: #0f172a; text-align: right;"">إجمالي قيمة الطلب الملغي:</td>
                        <td style=""padding: 8px 0; font-size: 16px; font-weight: 800; color: #b91c1c; text-align: left;"">{order.TotalAmount:N2} {order.Currency}</td>
                    </tr>
                </table>
            </div>
        </div>
        <div class=""footer"">
            © {DateTime.UtcNow.Year} NexaMart. نأمل أن نراك مجدداً قريباً للتسوق معنا!
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, html, cancellationToken);
    }
}
