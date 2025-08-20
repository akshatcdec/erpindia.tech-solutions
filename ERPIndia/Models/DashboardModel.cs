using System;

namespace ERPIndia.Models
{
    /// <summary>
    /// Dashboard model class.
    /// </summary>
    public class DashboardModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last login date.
        /// </summary>
        /// <value>
        /// The last login date.
        /// </value>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>
        /// The IP address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the admin user count.
        /// </summary>
        /// <value>
        /// The admin user count.
        /// </value>
        public string AdminUserCount { get; set; }

        /// <summary>
        /// Gets or sets the content template count.
        /// </summary>
        /// <value>
        /// The content template count.
        /// </value>
        public string ContentTemplateCount { get; set; }

        /// <summary>
        /// Gets or sets the email template count.
        /// </summary>
        /// <value>
        /// The email template count.
        /// </value>
        public string EmailTemplateCount { get; set; }

        #endregion
    }
}