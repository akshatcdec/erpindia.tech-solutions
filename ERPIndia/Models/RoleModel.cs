using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    /// <summary>
    /// Role model class.
    /// </summary>
    public class RoleModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        /// <value>
        /// The role id.
        /// </value>
        [Display(Name = "Role Id")]
        public long RoleId { get; set; }

        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        /// <value>
        /// The role name.
        /// </value>
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        /// <summary>
        /// Gets or sets the total record count.
        /// </summary>
        /// <value>
        /// The total record count.
        /// </value>
        [Display(Name = "Total Record Count")]
        public int TotalRecordCount { get; set; }

        /// <summary>
        /// Gets or sets the duplicate column.
        /// </summary>
        /// <value>
        /// The duplicate column.
        /// </value>
        [Display(Name = "Duplicate Column")]
        public string DuplicateColumn { get; set; }

        #endregion
    }
}
