using System.ComponentModel.DataAnnotations;

namespace NexaMart.Web.Models;

public enum ComplaintType
{
    [Display(Name = "General Inquiry")]
    GeneralInquiry = 1,

    [Display(Name = "Order Delivery Delay")]
    DeliveryDelay = 2,

    [Display(Name = "Damaged or Defective Product")]
    DamagedProduct = 3,

    [Display(Name = "Payment or Refund Issue")]
    PaymentIssue = 4,

    [Display(Name = "Service Complaint / Escalation")]
    ServiceComplaint = 5,

    [Display(Name = "Other")]
    Other = 6
}

public enum ComplaintPriority
{
    [Display(Name = "Normal")]
    Normal = 1,

    [Display(Name = "High")]
    High = 2,

    [Display(Name = "Urgent")]
    Urgent = 3
}

/// <summary>
/// Model for Contact Us inquiries and customer complaints / feedback.
/// </summary>
public class ContactComplaintViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [Display(Name = "Phone Number (Optional)")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Subject must be between 3 and 150 characters.")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select the type of inquiry or complaint.")]
    [Display(Name = "Inquiry / Complaint Category")]
    public ComplaintType InquiryType { get; set; } = ComplaintType.GeneralInquiry;

    [Display(Name = "Order Number (Optional)")]
    [StringLength(50, ErrorMessage = "Order number cannot exceed 50 characters.")]
    public string? OrderNumber { get; set; }

    [Display(Name = "Priority Level")]
    public ComplaintPriority Priority { get; set; } = ComplaintPriority.Normal;

    [Required(ErrorMessage = "Please enter your message or complaint details.")]
    [StringLength(3000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 3000 characters.")]
    [Display(Name = "Message or Complaint Details")]
    public string Message { get; set; } = string.Empty;
}
