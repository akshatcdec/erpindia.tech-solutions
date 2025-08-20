using ERPIndia.Class.Helper;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models.SystemSettings
{
    public class SystemSettingModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the setting id.
        /// </summary>
        /// <value>
        /// The setting id.
        /// </value>
        public long SettingId { get; set; }

        /// <summary>
        /// Gets or sets the site title.
        /// </summary>
        /// <value>
        /// The site title.
        /// </value>
        [Required]
        [Display(Name = "Site Title")]
        public string SiteTitle { get; set; }

        /// <summary>
        /// Gets or sets the copyright text.
        /// </summary>
        /// <value>
        /// The copyright text.
        /// </value>
        [Required]
        [Display(Name = "Copyright Text")]
        public string CopyrightText { get; set; }

        /// <summary>
        /// Gets or sets the admin email.
        /// </summary>
        /// <value>
        /// The admin email.
        /// </value>
        [Required]
        [RegularExpression(StringConstants.EmailRegEx, ErrorMessage = "Enter valid email.")]
        [Display(Name = "Admin Email")]
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets the support email.
        /// </summary>
        /// <value>
        /// The support email.
        /// </value>
        [RegularExpression(StringConstants.EmailRegEx, ErrorMessage = "Enter valid email.")]
        public string SupportEmail { get; set; }

        /// <summary>
        /// Gets or sets the toll free no.
        /// </summary>
        /// <value>
        /// The toll free no.
        /// </value>
        [RegularExpression(StringConstants.DigitRegEx, ErrorMessage = "Enter valid toll free number.")]
        public string TollFreeNo { get; set; }

        /// <summary>
        /// Gets or sets the SMTP host.
        /// </summary>
        /// <value>
        /// The SMTP host.
        /// </value>
        [Required]
        [Display(Name = "SMTP Host")]
        public string SMTPHost { get; set; }

        /// <summary>
        /// Gets or sets the SMTP port.
        /// </summary>
        /// <value>
        /// The SMTP port.
        /// </value>
        [Required]
        [RegularExpression(StringConstants.DigitRegEx, ErrorMessage = "Enter valid port.")]
        [Display(Name = "SMTP Port")]
        public int SMTPPort { get; set; }

        /// <summary>
        /// Gets or sets the name of the SMTP user.
        /// </summary>
        /// <value>
        /// The name of the SMTP user.
        /// </value>
        public string SMTPUserName { get; set; }

        /// <summary>
        /// Gets or sets the SMTP password.
        /// </summary>
        /// <value>
        /// The SMTP password.
        /// </value>
        public string SMTPPassword { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize { get; set; }
        public int FYearId { get; set; }

        /// <summary>
        /// Gets or sets the contact email.
        /// </summary>
        /// <value>
        /// The contact email.
        /// </value>
        [RegularExpression(StringConstants.EmailRegEx, ErrorMessage = "Enter valid email.")]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the contact no.
        /// </summary>
        /// <value>
        /// The contact no.
        /// </value>
        [RegularExpression(StringConstants.DigitRegEx, ErrorMessage = "Enter valid contact number.")]
        public string ContactNo { get; set; }

        #endregion
    }
}