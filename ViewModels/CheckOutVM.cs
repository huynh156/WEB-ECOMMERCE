using System.ComponentModel.DataAnnotations;

namespace FashionHubWeb.ViewModels
{
    public class CheckOutVM
    {
        public bool SameAddress { get; set; }
        
        [Required(ErrorMessage = "Full Name is required.")]
        public string? FullName { get; set; }
        
        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }
        
        [Required(ErrorMessage = "Phone Number is required.")]
        public int? PhoneNumber { get; set; }
        
        public string? Notes { get; set; }
        
        public string? CouponCode { get; set; }
    }
}
