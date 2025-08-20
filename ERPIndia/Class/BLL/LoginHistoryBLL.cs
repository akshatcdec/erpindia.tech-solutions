using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    /// <summary>
    /// Login history BLL class.
    /// </summary>
    public class LoginHistoryBLL
    {
        #region Public Methods

        /// <summary>
        /// Get Login History based on search and page size values
        /// </summary>
        /// <param name="searchField">field on which search perform</param>
        /// <param name="searchValue">value to be search</param>
        /// <param name="sortField">field to be sort</param>
        /// <param name="sortOrder">order of sort</param>
        /// <param name="pageNo">current page index</param>
        /// <param name="pageSize">size of page</param>
        /// <returns>list of filtered Login History</returns>
        public static List<SystemLoginHistoryModel> GetLoginHistory(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (LoginHistoryDAL loginHistoryDAL = new LoginHistoryDAL())
            {
                return loginHistoryDAL.GetLoginHistory(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        /// <summary>
        /// Saves the login history.
        /// </summary>
        /// <param name="loginHistory">The login history.</param>
        /// <returns>
        /// Returns login history id if success else error code.
        /// </returns>
        public static long SaveLoginHistory(SystemLoginHistoryModel loginHistory)
        {
            using (LoginHistoryDAL loginHistoryDAL = new LoginHistoryDAL())
            {
                return loginHistoryDAL.SaveLoginHistory(loginHistory);
            }
        }

        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public static int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            using (LoginHistoryDAL loginHistoryDAL = new LoginHistoryDAL())
            {
                return loginHistoryDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }

        #endregion
    }
}