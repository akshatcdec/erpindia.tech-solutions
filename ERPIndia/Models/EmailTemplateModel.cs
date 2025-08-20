using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace ERPIndia.Models
{
    /// <summary>
    /// Email template model class.
    /// </summary>
    public class EmailTemplateModel
    {

        public EmailTemplateModel()
        {
            this.IsActive = true;
        }
        #region Properties

        /// <summary>
        /// Gets or sets the email template id.
        /// </summary>
        /// <value>
        /// The email template id.
        /// </value>
        public long EmailTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        /// 
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        /// 
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        /// 
        [Required]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        /// 
        [Required]
        [EmailAddress]
        [Display(Name = "From Email")]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        /// 
        [Required]
        [AllowHtml]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        /// 
        [Required]
        [AllowHtml]
        public string Signature { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        /// 
        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the total record count.
        /// </summary>
        /// <value>
        /// The total record count.
        /// </value>
        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }

        #endregion
    }
}