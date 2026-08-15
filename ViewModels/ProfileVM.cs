using System.ComponentModel.DataAnnotations;

namespace FashionHubWeb.ViewModels
{
    public class ProfileVM
    {
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Phone number must be between 9 and 11 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Role { get; set; } = null!;

        // Change Password (Optional)
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(3, ErrorMessage = "New password must be at least 3 characters.")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}
