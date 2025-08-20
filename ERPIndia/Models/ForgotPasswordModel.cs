using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    /// <summary>
    /// Forgot password model class.
    /// </summary>
    public class ForgotPasswordModel
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the copy right.
        /// </summary>
        /// <value>
        /// The copy right.
        /// </value>
        public string CopyRight { get; set; }
    }
}