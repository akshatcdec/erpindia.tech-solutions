using ERPIndia.Class.Helper;
using System;

namespace ERPIndia.Models
{
    /// <summary>
    /// Login history model class.
    /// </summary>
    public class SystemLoginHistoryModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemLoginHistoryModel"/> class.
        /// </summary>
        public SystemLoginHistoryModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemLoginHistoryModel"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="action">The action.</param>
        public SystemLoginHistoryModel(long userId, string ipAddress, SystemLoginHistoryAction action)
        {
            this.UserId = userId;
            this.IPAddress = ipAddress;
            this.Action = action;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the login history id.
        /// </summary>
        /// <value>
        /// The login history id.
        /// </value>
        public long LoginHistoryId { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public SystemLoginHistoryAction Action { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>
        /// The IP address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the action date.
        /// </summary>
        /// <value>
        /// The action date.
        /// </value>
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Gets or sets the total record count.
        /// </summary>
        /// <value>
        /// The total record count.
        /// </value>
        public int TotalRecordCount { get; set; }

        #endregion
    }
}