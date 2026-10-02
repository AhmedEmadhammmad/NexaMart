using System.ComponentModel.DataAnnotations;
using NexaMart.Domain.Enums;

namespace NexaMart.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email or Staff ID is required.")]
    [MaxLength(150, ErrorMessage = "Identifier cannot exceed 150 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9@._+-]+$", ErrorMessage = "Identifier contains invalid characters.")]
    [Display(Name = "Email or Staff ID")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}

public class StaffLoginViewModel
{
    [Required(ErrorMessage = "Staff ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid positive Staff ID.")]
    [Display(Name = "Staff ID")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Role selection is required.")]
    [Display(Name = "Account Role")]
    public UserRoleType SelectedRole { get; set; } = UserRoleType.Admin;

    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s.'-]+$", ErrorMessage = "Full Name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address with a valid domain (e.g. user@domain.com).")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Please enter a valid phone number (7 to 20 digits).")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._-])[A-Za-z\d@$!%*?&#._-]{8,}$", 
        ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&#._-).")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s.'-]+$", ErrorMessage = "Full Name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Please enter a valid phone number (7 to 20 digits).")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Account Role")]
    public UserRoleType RoleType { get; set; }

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }

    // Optional password change fields
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._-])[A-Za-z\d@$!%*?&#._-]{8,}$", 
        ErrorMessage = "New password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&#._-).")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "New passwords do not match.")]
    [Display(Name = "Confirm New Password")]
    public string? ConfirmNewPassword { get; set; }
}

public class VerifyEmailViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the 6-digit verification code.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must consist of 6 digits.")]
    [Display(Name = "6-Digit Verification Code")]
    public string OtpCode { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the 6-digit recovery code.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Recovery code must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Recovery code must consist of 6 digits.")]
    [Display(Name = "6-Digit Recovery Code")]
    public string OtpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._-])[A-Za-z\d@$!%*?&#._-]{8,}$", 
        ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&#._-).")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm New Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
