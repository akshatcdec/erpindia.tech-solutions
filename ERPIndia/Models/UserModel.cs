using ERPIndia.Class.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ERPIndia.Models
{
    /// <summary>
    /// User model class.
    /// </summary>
    public class UserModel
    {

        public UserModel()
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
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [Required]
        [Display(Name = "Operator Name")]
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

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>

        [Display(Name = "Email")]
        public string Email { get; set; }

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
        
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        /// <value>
        /// The role id.
        /// </value>

        [Display(Name = "Role")]
        public long? RoleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        /// <value>
        /// The name of the role.
        /// </value>
        public string RoleName { get; set; }

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

        public List<TransportMonth> TransportMonths { get; set; }
        public List<TransportMonth> GetDefaultMonths()
        {
            return new List<TransportMonth>
        {
            new TransportMonth { monthId = 4, month_name = "Apr", Status = false },
            new TransportMonth { monthId = 5, month_name = "May", Status = false },
            new TransportMonth { monthId = 6, month_name = "Jun", Status = false },
            new TransportMonth { monthId = 7, month_name = "Jul", Status = false },
            new TransportMonth { monthId = 8, month_name = "Aug", Status = false },
            new TransportMonth { monthId = 9, month_name = "Sep", Status = false },
            new TransportMonth { monthId = 10, month_name = "Oct", Status = false },
            new TransportMonth { monthId = 11, month_name = "Nov", Status = false },
            new TransportMonth { monthId = 12, month_name = "Dec", Status = false },
            new TransportMonth { monthId = 1, month_name = "Jan", Status = false },
            new TransportMonth { monthId = 2, month_name = "Feb", Status = false },
            new TransportMonth { monthId = 3, month_name = "Mar", Status = false }
        };
        }

        /// <summary>
        /// Sets the transport months based on database data.
        /// </summary>
        /// <param name="dbMonths">The transport months from the database.</param>
        public void SetTransportMonths(List<TransportMonth> dbMonths)
        {
            if (dbMonths == null || !dbMonths.Any())
            {
                return; // Keep default months
            }

            foreach (var dbMonth in dbMonths)
            {
                var month = TransportMonths.FirstOrDefault(m => m.monthId == dbMonth.monthId);
                if (month != null)
                {
                    month.Status = dbMonth.Status;
                }
            }
        }

        /// <summary>
        /// Converts the UserModel transport months to database entities.
        /// </summary>
        /// <returns>A list of SchoolTransportMonths entities.</returns>
        public List<TransportMonth> ToTransportMonthsEntities()
        {
            if (TransportMonths == null || !TransportMonths.Any())
            {
                return new List<TransportMonth>();
            }

            return TransportMonths.Select(m => new TransportMonth
            {
                monthId = m.monthId,
                month_name = m.month_name,
                Status = m.Status,
                ClientId = m.ClientId,
                SchoolCode = m.SchoolCode
            }).ToList();
        }
        #endregion
    }
    public class TransportMonth
    {
        public int monthId { get; set; }
        public string month_name { get; set; }
        public bool Status { get; set; }
        public int ClientId { get; set; }
        public int SchoolCode { get; set; }
    }
    public class ImportDataModel
    {

        [Display(Name = "Excel File")]
        public string ProfilePic { get; set; }

        public string ExcelFile { get; set; }
    }
}