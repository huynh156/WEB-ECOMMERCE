using System.ComponentModel.DataAnnotations;

namespace FashionHubWeb.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterVM
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Phone number must be between 9 and 11 digits.")]
        public string PhoneNumber { get; set; } = null!;
    }
}
