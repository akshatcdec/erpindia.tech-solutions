using ERPIndia.Class.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models.SystemSettings
{
    public class SystemUsersModel
    {
        public SystemUsersModel()
        {
            this.IsActive = true;
        }
        #region Properties
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public long SystemUserId { get; set; }
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        public string MiddleName { get; set; }
        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName
        {
            get
            {
                return string.Concat(this.FirstName, ' ', this.LastName);
            }
        }
        /////// <summary>
        /////// Gets or sets the name of the user.
        /////// </summary>
        /////// <value>
        /////// The name of the user.
        /////// </value>
        ////[Required]
        ////[Display(Name = "User Name")]
        ////public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [Required]
        [RegularExpression(StringConstants.EmailRegEx, ErrorMessage = "Enter valid email.")]
        [Display(Name = "Email (User Name)")]
        public string Email { get; set; }

        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        /// <value>
        /// The role id.
        /// </value>
        [Display(Name = "Role")]
        public long? SystemRoleId { get; set; }
        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        /// <value>
        /// The name of the role.
        /// </value>
        public string SystemRoleName { get; set; }

        /// <summary>
        /// Gets or sets the school code.
        /// </summary>
        public string SchoolCode { get; set; }

        /// <summary>
        /// Gets or sets the school id.
        /// </summary>
        public long SchoolId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        public Guid TenantID { get; set; }

        /// <summary>
        /// Gets or sets the tenant name.
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// Gets or sets the tenant user ID.
        /// </summary>
        public Guid TenantUserId { get; set; }

        /// <summary>
        /// Gets or sets the active session ID.
        /// </summary>
        public Guid ActiveSessionID { get; set; }

        /// <summary>
        /// Gets or sets the active session year.
        /// </summary>
        public int ActiveSessionYear { get; set; }

        /// <summary>
        /// Create By.
        /// </summary>
        /// <value>
        ///   Created By
        /// </value>
        public long CreatedBy { get; set; }
        /// <summary>
        /// Gets or sets role Id.
        /// </summary>
        /// <value>
        /// the name of the company
        /// </value>
        [Display(Name = "School")]
        public long? CompanyId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
        /// <summary>
        /// Gets or sets the total record count.
        /// </summary>
        /// <value>
        /// The total record count.
        /// </value>
        public int TotalRecordCount { get; set; }
        /// <summary>
        /// Gets or sets the duplicate column.
        /// </summary>
        /// <value>
        /// The duplicate column.
        /// </value>
        public string DuplicateColumn { get; set; }
        [Display(Name = "Profile Picture")]
        public string ProfilePic { get; set; }
        #endregion
    }
}