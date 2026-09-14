using System.ComponentModel.DataAnnotations;
using NexaMart.Domain.Enums;

namespace NexaMart.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email or Staff ID is required.")]
    [Display(Name = "Email or Staff ID")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
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
    [Display(Name = "Staff ID")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Role selection is required.")]
    [Display(Name = "Account Role")]
    public UserRoleType SelectedRole { get; set; } = UserRoleType.Admin;

    [Required(ErrorMessage = "Password is required.")]
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
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
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
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
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

    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "New passwords do not match.")]
    [Display(Name = "Confirm New Password")]
    public string? ConfirmNewPassword { get; set; }
}
