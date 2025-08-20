using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models.RazorPay
{
    public class PaymentRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int Amount { get; set; }

    }
}